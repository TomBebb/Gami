using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Gami.Core.Models;
using Gami.Desktop.Plugins;
using Serilog;

namespace Gami.Desktop.Db;

public static class DbOps
{
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
                        .Select(g => new GameLibraryRef()
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
                    var achievements = await scanner.Scan(g);
                    foreach (var a in achievements)
                    {
                        var id =
                            $"{type}:{g.LibraryId}::{a.LibraryId}";
                        Log.Debug("Process {Id}", id);
                    }

                    Log.Debug("Scanning achievements {Data}",
                        JsonSerializer.Serialize(achievements.Select(a => a.LibraryId)));
                    await using var db = new GamiContext();
                    await db.BulkInsertAsync(achievements.Select(a => new Achievement()
                    {
                        LockedIcon = a.LockedIcon,
                        UnlockedIcon = a.UnlockedIcon,
                        Id = $"{type}:{g.LibraryId}::{a.LibraryId}",
                        Name = a.Name,
                        LibraryId = a.LibraryId
                    }));
                }));
        }
    }

    private static async ValueTask ScanAchievementsProgress()
    {
        foreach (var (type, scanner) in GameExtensions.AchievementsByName)
        {
            ImmutableArray<GameLibraryRef> gamesToScan;
            await using (var db = new GamiContext())
            {
                gamesToScan = db.Games
                    .Where(g => g.LibraryType == type)
                    .Select(g => new GameLibraryRef()
                    {
                        LibraryType = g.LibraryType,
                        LibraryId = g.LibraryId,
                        Name = g.Name
                    }).ToImmutableArray();
            }

            await Task.WhenAll(
                gamesToScan.Select(async g =>
                {
                    await using var db = new GamiContext();
                    var achievementsProgress = await scanner.ScanProgress(g);

                    await db.BulkInsertOrUpdateAsync(achievementsProgress);
                }));
        }
    }

    public static async ValueTask DoScan()
    {
        await using var db = new GamiContext();
        foreach (var scanner in GameExtensions.ScannersByName)
        {
            Log.Information("Scan {Name} apps", scanner.Key);
            var fetched = new ConcurrentBag<IGameLibraryMetadata>();
            await foreach (var item in scanner.Value.Scan().ConfigureAwait(false))
                fetched
                    .Add(item);

            Log.Debug(
                "{Name} apps {Apps}", scanner.Key, JsonSerializer.Serialize(fetched.Select(f => f.Name),
                    SerializerSettings
                        .JsonOptions));
            await db.BulkInsertOrUpdateAsync(fetched.Select(g => new Game()
            {
                Id = $"{scanner.Key}:{g.LibraryId}",
                Name = g.Name,
                InstallStatus = g.InstallStatus,
                Description = ""
            }));
        }

        return;

        Task.Run(async () =>
        {
            Log.Debug("Scanning missing achievement data");
            await ScanMissingAchievementsData();
            Log.Debug("Scanned missing achievement data; scanning progress");
            await ScanAchievementsProgress();

            Log.Debug("Scanned achievements progress");
        });
    }
}