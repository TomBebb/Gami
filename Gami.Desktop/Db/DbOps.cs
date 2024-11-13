using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Gami.Core;
using Gami.Core.Ext;
using Gami.Core.Models;
using Gami.Desktop.Addons;
using Gami.Desktop.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Gami.Desktop.Db;

public static class DbOps
{
    public static async ValueTask ScanAchievementsData(Action<float> onProgress)
    {
        await Task.WhenAll(
            GamiAddons.AchievementsByName.Values.Select(async g => { await ScanAchievementsData(g, onProgress); }));
    }

    public static async ValueTask ScanAchievementsData(IGameAchievementScanner scanner, Action<float> onProgress)
    {
        await using var db = new GamiContext();

        ImmutableArray<GameLibraryRef> gamesMissingAchievements =
        [
            .. db.Games
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
        foreach (var (_, scanner) in GamiAddons.MetadataScannersByName)
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
                .. db.Games.Where(g => g.Description == "").Select(g => new GameLibraryRef
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
                .. db.Games
                    .Where(g => g.LibraryType == key)
                    .Where(g => g.Description == "")
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
        curr.ReleaseDate = metadata.ReleaseDate?.ToDateTime(new TimeOnly(0, 0)) ?? DateTime.UnixEpoch;

        if (metadata.Genres != null)
        {
            curr.Genres = new List<Genre>(metadata.Genres?.Length ?? 0);
            foreach (var cn in metadata.Genres!)
            {
                var existing = await db.Genres.Where(g => g.Name == cn).FirstOrDefaultAsync();
                curr.Genres.Add(existing ?? new Genre { Name = cn });
            }
        }

        if (metadata.Developers != null)
        {
            curr.Developers = new List<Developer>(metadata.Developers?.Length ?? 0);
            foreach (var cn in metadata.Developers!)
            {
                var existing = await db.Developers.Where(g => g.Name == cn).FirstOrDefaultAsync();
                curr.Developers.Add(existing ?? new Developer { Name = cn });
            }
        }

        if (metadata.Publishers != null)
        {
            curr.Publishers = new List<Publisher>(metadata.Publishers?.Length ?? 0);
            foreach (var cn in metadata.Publishers!)
            {
                var existing = await db.Publishers.Where(g => g.Name == cn).FirstOrDefaultAsync();
                curr.Publishers.Add(existing ?? new Publisher { Name = cn });
            }
        }

        if (metadata.Series != null)
        {
            curr.Series = new List<Series>(metadata.Series?.Length ?? 0);
            foreach (var cn in metadata.Series!)
            {
                var existing = await db.Series.Where(g => g.Name == cn).FirstOrDefaultAsync();
                curr.Series.Add(existing ?? new Series { Name = cn });
            }
        }

        await db.SaveChangesAsync();

        Log.Debug("Steam saved for {Game}", game);
    }

    // ReSharper disable once UnusedMember.Local 
    public static async ValueTask ScanAchievementsProgress(AchievementsSettings settings)
    {
        Log.Information("Scanning achievements progress");
        foreach (var (type, scanner) in GamiAddons.AchievementsByName)
        {
            Log.Information("Scanning achievement progress for ");
            ImmutableArray<AchievementGameLibraryRef> gamesToScan;
            await using (var db = new GamiContext())
            {
                var filteredGames =
                    db.Games
                        .Where(g => g.LibraryType == type)
                        .Where(g => db.Achievements.Any(a => a.GameId == g.Id));
                if (settings.OnlyScanInstalled)
                    filteredGames = filteredGames.Where(g => g.InstallStatus == GameInstallStatus.Installed);
                gamesToScan =
                [
                    .. filteredGames
                        .Select(g => new AchievementGameLibraryRef
                        {
                            LibraryType = g.LibraryType,
                            LibraryId = g.LibraryId,
                            Name = g.Name,
                            TotalAchievements = g.Achievements.Count
                        })
                ];
            }


            Log.Information("Scanning achievements progress for: {Total} games", gamesToScan.Length);
            await Task.WhenAll(
                gamesToScan.Select(async g =>
                {
                    Log.Information("Scanning achievements progress for: {Name}", g.Name);
                    await using var db = new GamiContext();

                    var progress = new List<AchievementProgress>(g.TotalAchievements);

                    await foreach (var pr in scanner.ScanProgress(g)) progress.Add(pr);

                    Log.Information("Inserting achievements progress for: {Name}; Total: {Total}", g.Name,
                        progress.Count);
                    await db.BulkInsertOrUpdateAsync(progress);
                    Log.Information("Inserted achievements progress for: {Name}; Total: {Total}", g.Name,
                        progress.Count);
                }));
        }
    }

    public static ValueTask ScanLibrary(string key)
    {
        return ScanLibrary(GamiAddons.ScannersByName[key]);
    }

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
                LastPlayed = item.LastPlayed,
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

    private class AchievementGameLibraryRef : GameLibraryRef
    {
        public int TotalAchievements { get; set; }
    }
}