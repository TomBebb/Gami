using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EFCore.BulkExtensions;
using Gami.Core;
using Gami.Core.Models;
using Gami.Desktop.Db;
using Gami.Desktop.Plugins;
using Gami.Desktop.ViewModels;
using Gami.Desktop.Views;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Gami.Desktop;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private static async ValueTask ScanMissingAchievementsData()
    {
        foreach (var (type, scanner) in GameExtensions.AchievementsByName)
        {
            await using var db = new GamiContext();
            var gamesMissingAchievements = db.Games
                .Where(g => g.Achievements.Count == 0)
                .Where(g => g.LibraryType == type)
                .Select(g => new GameLibraryRef()
                {
                    LibraryType = g.LibraryType,
                    LibraryId = g.LibraryId,
                    Name = g.Name
                }).ToImmutableArray();

            await Task.WhenAll(
                gamesMissingAchievements.Select(async g =>
                {
                    await using var db = new GamiContext();
                    var achievements = await scanner.Scan(g);
                    foreach (var a in achievements)
                    {
                        var id =
                            $"{type}:{g.LibraryId}::{a.LibraryId}";
                        Log.Debug("Process {Id}", id);
                    }

                    Log.Debug("Scanning achievements {Data}",
                        JsonSerializer.Serialize(achievements));
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
            await using var db = new GamiContext();

            var gamesToScan = db.Games
                .Where(g => g.LibraryType == type)
                .Select(g => new GameLibraryRef()
                {
                    LibraryType = g.LibraryType,
                    LibraryId = g.LibraryId,
                    Name = g.Name
                }).ToImmutableArray();

            await Task.WhenAll(
                gamesToScan.Select(async g =>
                {
                    await using var db = new GamiContext();
                    var achievementsProgress = await scanner.ScanProgress(g);

                    await db.BulkInsertAsync(achievementsProgress);
                }));
        }
    }

    private static async ValueTask DoScan()
    {
        foreach (var scanner in GameExtensions.ScannersByName)
        {
            Log.Information("Scan {Name} apps", scanner.Key);
            var fetched = new ConcurrentBag<IGameLibraryRef>();
            await foreach (var item in scanner.Value.Scan().ConfigureAwait(false))
            {
                if (fetched.Count >= 1)
                    break;
                fetched
                    .Add(item);
            }

            Log.Debug(
                "{Name} apps {Apps}", scanner.Key, JsonSerializer.Serialize(fetched,
                    SerializerSettings
                        .JsonOptions));
            await using var db = new GamiContext();

            await db.BulkInsertOrUpdateAsync(fetched.Select(g => new Game()
            {
                Id = $"{scanner.Key}:{g.LibraryId}",
                Name = g.Name,
                InstallStatus = g.InstallStatus,
                Description = ""
            }));
        }

        Log.Debug("Scanning missing achievement data");
        await ScanMissingAchievementsData();
        Log.Debug("Scanned missing achievement data; scanning progress");
        await ScanAchievementsProgress();

        Log.Debug("Scanned achievements progress");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (!Design.IsDesignMode)
            {
                Log.Information("Ensure local app dir exists");
                Directory.CreateDirectory(Consts.BasePluginDir);
                using (DbContext context = new GamiContext())
                {
                    Log.Information("Ensure DB created");
                    context.Database.EnsureCreated();
                }

                Log.Information("Save changes");

                DoScan().GetAwaiter().GetResult();
                /*Task.Run(() =>
                    DoScan().AsTask());
                */
                Log.Information("Saved changes");
            }

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
            if (!Design.IsDesignMode && !Directory.Exists(Consts.AppDir))
                Directory.CreateDirectory(Consts.AppDir);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}