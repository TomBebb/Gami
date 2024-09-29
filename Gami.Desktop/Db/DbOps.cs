﻿using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Gami.Core;
using Gami.Core.Ext;
using Gami.Core.Models;
using Gami.Desktop.Plugins;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Gami.Desktop.Db;

public static class DbOps
{
    private static async ValueTask ScanMissingIcons()
    {
        await using var db = new GamiContext();

        foreach (var item in db.Games
                     .Where(g => g.IconUrl == null)
                     .Select(g => new GameLibraryRef
                         { Name = g.Name, LibraryId = g.LibraryId, LibraryType = g.LibraryType }))
        {
            var scanner = GameExtensions.IconLookupByName[item.LibraryType];
            var icon = await scanner.LookupIcon(item);
            var mapped = new Game
            {
                Id = $"{item.LibraryType}:{item.LibraryId}",
                IconUrl = icon
            };
            db.Games.Attach(mapped);
            db.Entry(mapped).Property(x => x.IconUrl).IsModified = true;

            await db.SaveChangesAsync();
        }
    }

    public static async ValueTask ScanAchievementsData(string key)
    {
        await ScanAchievementsData(GameExtensions.AchievementsByName[key]);
    }

    public static async ValueTask ScanAchievementsData()
    {
        await Task.WhenAll(GameExtensions.AchievementsByName.Values.Select(async g => await ScanAchievementsData(g)));
    }

    public static async ValueTask ScanAchievementsData(IGameAchievementScanner scanner)
    {
        await using var db = new GamiContext();

        ImmutableArray<GameLibraryRef> gamesMissingAchievements =
        [
            ..db.Games
                .Where(g => !db.Achievements.Where(a => a.GameId == g.Id).Any())
                .Where(g => g.LibraryType == scanner.Type)
                .Select(g => new GameLibraryRef
                {
                    LibraryType = g.LibraryType,
                    LibraryId = g.LibraryId,
                    Name = g.Name
                })
        ];

        var achievements = new ConcurrentBag<Achievement>();

        await Task.WhenAll(
            gamesMissingAchievements.Select(async g =>
            {
                Log.Information("Scanning achievements..");

                await foreach (var achievement in scanner.Scan(g)) achievements.Add(achievement);

                Log.Information("Scanned achievements for {Game}", g.Name);
            }));
        Log.Information("Inserting achievements ");

        await db.BulkInsertOrUpdateAsync(achievements);
    }

    private static async ValueTask<GameMetadata> GetMetadata(GameLibraryRef game)
    {
        var metadata = new GameMetadata();
        foreach (var (_, scanner) in GameExtensions.MetadataScannersByName)
            metadata = metadata.Combine(await scanner.ScanMetadata(game));
        return metadata;
    }

    private static async ValueTask<int> GetOrCreate<T>(this DbContext context, DbSet<T> set, string value) where T :
        NamedIdItem, new()
    {
        Log.Debug("{Func} Value: {Val}", nameof(GetOrCreate), value);
        var myId = await set.Where(v => v.Name == value)
            .Select(v => v.Id)
            .Cast<int?>()
            .FirstOrDefaultAsync();

        if (myId != null)
            return myId.Value;

        var obj = new T { Name = value };

        await set.AddAsync(obj);
        await context.SaveChangesAsync();
        return obj.Id;
    }

    public static async ValueTask ScanMetadata()
    {
        ImmutableArray<GameLibraryRef> refs;
        await using (var db = new GamiContext())
        {
            refs = db.Games.Select(g => new GameLibraryRef
            {
                LibraryId = g.LibraryId,
                Name = g.Name,
                LibraryType = g.LibraryType
            }).ToImmutableArray();
        }

        await Task.WhenAll(refs.Select(async vm => await ScanMetadata(vm)));
    }

    public static async ValueTask ScanMetadata(string key)
    {
        ImmutableArray<GameLibraryRef> refs;
        await using (var db = new GamiContext())
        {
            refs = db.Games
                .Where(g => g.LibraryType == key)
                .Select(g => new GameLibraryRef
                {
                    LibraryId = g.LibraryId,
                    Name = g.Name,
                    LibraryType = g.LibraryType
                }).ToImmutableArray();
        }

        await Task.WhenAll(refs.Select(async vm => await ScanMetadata(vm)));
    }

    public static async ValueTask ScanMetadata(GameLibraryRef game)
    {
        await using var db = new GamiContext();
        var metadata = await GetMetadata(game);

        var curr = await db.Games.FindAsync($"{game.LibraryType}:{game.LibraryId}");
        if (curr == null)
            throw new ApplicationException($"No matching game ref found in DB for {game.LibraryId}");

        curr.Description = metadata.Description;
        curr.Genres = metadata.Genres.Select(v => new Genre
        {
            Name = v
        }).ToList();
        curr.Developers = metadata.Developers.Select(v => new Developer
        {
            Name = v
        }).ToList();
        curr.Publishers = metadata.Publishers.Select(v => new Publisher
        {
            Name = v
        }).ToList();
        curr.Series = metadata.Series.Select(v => new Series
        {
            Name = v
        }).ToList();
        await db.SaveChangesAsync();

        Log.Debug("Steam saved");
    }

// ReSharper disable once UnusedMember.Local
    private static async ValueTask ScanAchievementsProgress()
    {
        foreach (var (type, scanner) in GameExtensions.AchievementsByName)
        {
            ImmutableArray<GameLibraryRef> gamesToScan;
            await using (var db = new GamiContext())
            {
                gamesToScan =
                [
                    ..db.Games
                        .Where(g => g.LibraryType == type)
                        .Where(g => g.Achievements.Count != 0)
                        .Select(g => new GameLibraryRef
                        {
                            LibraryType = g.LibraryType,
                            LibraryId = g.LibraryId,
                            Name = g.Name
                        })
                ];
            }


            await Task.WhenAll(
                gamesToScan.Select(async g =>
                {
                    Log.Information("Scanning achievements progress for: {Name}", g.Name);
                    await using var db = new GamiContext();

                    await foreach (var pr in scanner.ScanProgress(g)) db.AchievementsProgresses.Update(pr);
                    await db.SaveChangesAsync();
                    Log.Information("Scanned achievements progress for: {Name}", g.Name);
                }));
        }
    }

    public static async ValueTask ScanLibrary(IGameLibraryScanner scanner)
    {
        await using var db = new GamiContext();
        await foreach (var item in scanner.Scan().ConfigureAwait(false))
        {
            Lazy<string> WithPrefix(string name)
            {
                return new Lazy<string>(() => $"{item.LibraryType}_{item.LibraryId}_{name}");
            }

            if (await db.ExcludedGames.AnyAsync(eg =>
                    eg.LibraryId == item.LibraryId && eg.LibraryType == item.LibraryType))
            {
                Log.Debug("Skip excluded game {}", item.Name);
                continue;
            }

            var mapped = new Game
            {
                Id = $"{scanner.Type}:{item.LibraryId}",
                Name = item.Name,
                InstallStatus = item.InstallStatus,
                Description = "",
                Playtime = item.Playtime,
                IconUrl = await item.IconUrl.AutoDownloadUriOpt(WithPrefix("icon")),
                HeaderUrl = await item.HeaderUrl.AutoDownloadUriOpt(WithPrefix("header")),
                LogoUrl = await item.LogoUrl.AutoDownloadUriOpt(WithPrefix("logo")),
                HeroUrl = await item.HeroUrl.AutoDownloadUriOpt(WithPrefix("hero"))
            };
            if (await db.Games.AnyAsync(v => v.Id == mapped.Id))
            {
                db.Games.Attach(mapped);
                db.Entry(mapped).Property(x => x.Name).IsModified = true;
                db.Entry(mapped).Property(x => x.InstallStatus).IsModified = true;
                db.Entry(mapped).Property(x => x.Playtime).IsModified = true;
                db.Entry(mapped).Property(x => x.IconUrl).IsModified = true;
                db.Entry(mapped).Property(x => x.HeroUrl).IsModified = true;
                db.Entry(mapped).Property(x => x.HeaderUrl).IsModified = true;
                db.Entry(mapped).Property(x => x.LogoUrl).IsModified = true;
            }
            else
            {
                await db.AddAsync(mapped);
            }
        }

        await db.SaveChangesAsync();
    }

    public static async ValueTask ScanAllLibraries()
    {
        await Task.WhenAll(GameExtensions.ScannersByName.Values.Select(async v => await ScanLibrary(v)));
    }

    public static async ValueTask AutoScan()
    {
        await ScanAchievementsProgress();
    }
}