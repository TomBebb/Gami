using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
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

    // ReSharper disable once UnusedMember.Local
    private static async ValueTask ScanMissingAchievementsData()
    {
        foreach (var (type, scanner) in GameExtensions.AchievementsByName)
        {
            ImmutableArray<GameLibraryRef> gamesMissingAchievements;
            await using (var db = new GamiContext())
            {
                gamesMissingAchievements =
                [
                    ..db.Games
                        .Where(g => g.Achievements.Count == 0)
                        .Where(g => g.LibraryType == type)
                        .Select(g => new GameLibraryRef
                        {
                            LibraryType = g.LibraryType,
                            LibraryId = g.LibraryId,
                            Name = g.Name
                        })
                ];
            }

            await Task.WhenAll(
                gamesMissingAchievements.Select(async g =>
                {
                    Log.Information("Scanning achievements..");
                    var achievements = await scanner.Scan(g);

#if DEBUG
                    Log.Debug("Got achievements {Data}",
                        JsonSerializer.Serialize(achievements.Select(a => a.LibraryId)));

#endif
                    await using var db = new GamiContext();
                    Log.Information("Inserting achievements for {Game}", g.Name);
                    await db.BulkInsertOrUpdateAsync(achievements);


                    Log.Information("Inserted achievements for {Game}", g.Name);
                }));
        }
    }

    private static async ValueTask<GameMetadata> GetMetadata(GameLibraryRef game)
    {
        var metadata = new GameMetadata();
        foreach (var (_, scanner) in GameExtensions.MetadataScannersByName)
            metadata = metadata.Combine(await scanner.ScanMetadata(game));
        return metadata;
    }

    // ReSharper disable once UnusedMember.Local
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

    private static async ValueTask ScanMetadata(GameLibraryRef game)
    {
        await using var db = new GamiContext();
        var metadata = await GetMetadata(game);

        var curr = await db.Games.FindAsync($"{game.LibraryType}:{game.LibraryId}");
        if (curr == null)
            throw new ApplicationException($"No matching game ref found in DB for {game.LibraryId}");

        if (metadata.Description != null)
            curr.Description = metadata.Description;
        if (metadata.Genres != null)
            curr.Genres = (metadata.Genres ?? ImmutableArray<string>.Empty).Select(v => new Genre
            {
                Name = v
            }).ToList();
        if (metadata.Developers != null)
            curr.Developers = (metadata.Developers ?? ImmutableArray<string>.Empty).Select(v => new Developer
            {
                Name = v
            }).ToList();
        if (metadata.Publishers != null)
            curr.Publishers = (metadata.Publishers ?? ImmutableArray<string>.Empty).Select(v => new Publisher
            {
                Name = v
            }).ToList();
        if (metadata.Series != null)
            curr.Series = (metadata.Series ?? ImmutableArray<string>.Empty).Select(v => new Series
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
                    var achievementsProgress = await scanner.ScanProgress(g);

                    await db.BulkInsertOrUpdateAsync(achievementsProgress);
                    Log.Information("Scanned achievements progress for: {Name}", g.Name);
                }));
        }
    }

    public static async ValueTask DoScan()
    {
        await using (var db = new GamiContext())
        {
            foreach (var scanner in GameExtensions.ScannersByName)
            {
                Log.Information("Scan {Name} apps", scanner.Key);
                await foreach (var item in scanner.Value.Scan().ConfigureAwait(false))
                {
                    Lazy<string> WithPrefix(string name)
                    {
                        return new Lazy<string>(() => $"{item.LibraryType}_{item.LibraryId}_{name}");
                    }

                    var mapped = new Game
                    {
                        Id = $"{scanner.Key}:{item.LibraryId}",
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
        }

        _ = Task.Run(async () =>
        {
            Log.Debug("Scanning icons");
            await ScanMissingIcons();
            Log.Debug("Scanned icon");
        });
        _ = Task.Run(async () =>
        {
            Log.Debug("Scanning achievements");
            await ScanMissingAchievementsData();
            Log.Debug("Scanned achievements");
        });
        _ = Task.Run(async () =>
        {
            Log.Debug("Scanning metadata");
            await using var db = new GamiContext();

            await Task.WhenAll(db.Games.Select(v => new GameLibraryRef
            {
                LibraryId = v.LibraryId,
                LibraryType = v.LibraryType,
                Name = v.Name
            }).Select(v => ScanMetadata(v).AsTask()));

            Log.Debug("Scanned metadta");
        });
        Task.Run(async () =>
        {
            Log.Debug(" scanning progress");
            await ScanAchievementsProgress();

            Log.Debug("Scanned achievements progress");
        });
    }
}