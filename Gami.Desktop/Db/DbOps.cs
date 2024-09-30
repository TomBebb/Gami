using System;
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
    public static async ValueTask ScanAchievementsData(Action<float> onProgress)
    {
        await Task.WhenAll(
            GameExtensions.AchievementsByName.Values.Select(async g => { await ScanAchievementsData(g, onProgress); }));
    }

    public static async ValueTask ScanAchievementsData(IGameAchievementScanner scanner, Action<float> onProgress)
    {
        await using var db = new GamiContext();

        ImmutableArray<GameLibraryRef> gamesMissingAchievements =
        [
            ..db.Games
                // ReSharper disable once AccessToDisposedClosure
                .Where(g => !db.Achievements.Any(a => a.GameId == g.Id))
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
            gamesMissingAchievements.Select(async (g, index) =>
            {
                onProgress(index / (float)gamesMissingAchievements.Length);
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

    public static async ValueTask ScanMetadata(Action<float> onProgress)
    {
        ImmutableArray<GameLibraryRef> refs;
        await using (var db = new GamiContext())
        {
            refs =
            [
                ..db.Games.Select(g => new GameLibraryRef
                {
                    LibraryId = g.LibraryId,
                    Name = g.Name,
                    LibraryType = g.LibraryType
                })
            ];
        }

        await Task.WhenAll(refs.Select(async (vm, index) =>
        {
            onProgress(index / (float)refs.Length);
            await ScanMetadata(vm);
        }));
    }

    public static async ValueTask ScanMetadata(string key, Action<float> onProgress)
    {
        ImmutableArray<GameLibraryRef> refs;
        await using (var db = new GamiContext())
        {
            refs =
            [
                ..db.Games
                    .Where(g => g.LibraryType == key)
                    .Select(g => new GameLibraryRef
                    {
                        LibraryId = g.LibraryId,
                        Name = g.Name,
                        LibraryType = g.LibraryType
                    })
            ];
        }

        await Task.WhenAll(refs.Select(async (vm, index) =>
        {
            await ScanMetadata(vm);
            onProgress(index / (float)refs.Length);
        }));
    }

    private static async ValueTask ScanMetadata(GameLibraryRef game)
    {
        await using var db = new GamiContext();
        var metadata = await GetMetadata(game);

        var curr = await db.Games.FindAsync($"{game.LibraryType}:{game.LibraryId}");
        if (curr == null)
            throw new ApplicationException($"No matching game ref found in DB for {game.LibraryId}");

        curr.Description = metadata.Description ?? "";
        if (metadata.Genres != null)
            curr.Genres = metadata.Genres.Value.Select(v => new Genre
            {
                Name = v
            }).ToList();
        if (metadata.Developers != null)
            curr.Developers = metadata.Developers.Value.Select(v => new Developer
            {
                Name = v
            }).ToList();
        if (metadata.Publishers != null)
            curr.Publishers = metadata.Publishers.Value.Select(v => new Publisher
            {
                Name = v
            }).ToList();
        if (metadata.Series != null)
            curr.Series = metadata.Series.Value.Select(v => new Series
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

    public static ValueTask ScanLibrary(string key) => ScanLibrary(GameExtensions.ScannersByName[key]);

    public static async ValueTask ScanLibrary(IGameLibraryScanner scanner)
    {
        await using var db = new GamiContext();
        await foreach (var item in scanner.Scan().ConfigureAwait(false))
        {
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

            continue;

            Lazy<string> WithPrefix(string name)
            {
                return new Lazy<string>(() => $"{item.LibraryType}_{item.LibraryId}_{name}");
            }
        }

        await db.SaveChangesAsync();
    }

    public static async ValueTask AutoScan()
    {
        await ScanAchievementsProgress();
    }
}