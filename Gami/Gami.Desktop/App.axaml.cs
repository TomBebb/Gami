using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

    private static async ValueTask DoScan()
    {
        foreach (var scanner in GameExtensions.ScannersByName)
        {
            Log.Information("Scan {Name} apps", scanner.Key);
            var fetched = new ConcurrentBag<IGameLibraryRef>();
            var fetchedAchievements = new ConcurrentDictionary<string, Achievement>();
            await foreach (var item in scanner.Value.Scan().ConfigureAwait(false))
            {
                if (fetched.Count >= 1)
                    break;
                fetched
                    .Add(item);

                if (GameExtensions.AchievementsByName.TryGetValue(scanner.Key,
                        out var achievementScanner))
                    Log.Debug("Scan {Name} Achievements", scanner.Key);

                await foreach (var achievement in achievementScanner.Scan(item))
                    fetchedAchievements[item.LibraryId] = achievement;

                Log.Debug("got achievements for {Item}", item.LibraryId);
            }

            Log.Debug(
                "{Name} apps {Apps}", scanner.Key, JsonSerializer.Serialize(fetched,
                    SerializerSettings
                        .JsonOptions));
            Log.Debug(
                "{Name} achievements {Achievements}", scanner.Key,
                JsonSerializer.Serialize(fetchedAchievements,
                    SerializerSettings
                        .JsonOptions));
            {
                await using var db = new GamiContext();
                await db.BulkInsertOrUpdateAsync<Game>(fetched.Select(
                    f => new Game
                    {
                        InstallStatus = f.InstallStatus,
                        Name = f.Name,
                        LibraryId = f.LibraryId,
                        LibraryType = f.LibraryType,
                        Description = ""
                    }));
            }
        }
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