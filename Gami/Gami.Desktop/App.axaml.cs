using System;
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
using Gami.Core.Models;
using Gami.Desktop.Db;
using Gami.Desktop.Steam;
using Gami.Desktop.ViewModels;
using Gami.Desktop.Views;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Gami.Desktop;

public class App : Application
{
    public static readonly string AppDir =
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "gami");


    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private static async ValueTask DoScan()
    {
        var scanner = new SteamScanner();
        Log.Information("Scan steam apps");
        var fetched = new List<IGameLibraryRef>();
        await foreach (var item in scanner.Scan()) fetched.Add(item);
        Log.Debug($"Steam apps {JsonSerializer.Serialize(fetched, SerializerSettings.JsonOptions)}");
        {
            await using var db = new GamiContext();
            await db.BulkInsertAsync(fetched
                .Where(v => !db.Games.Any(g => g.LibraryId == v.LibraryId && g.LibraryType == v.LibraryType)).Select(
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

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (!Design.IsDesignMode)
            {
                Log.Information("Ensure local app dir exists");
                Directory.CreateDirectory(AppDir);
                using DbContext context = new GamiContext();
                Log.Information("Ensure DB created");
                context.Database.EnsureCreated();
                Log.Information("Save changes");

                DoScan().GetAwaiter().GetResult();

                context.SaveChanges();
                Log.Information("Saved changes");
            }

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
            if (!Design.IsDesignMode && !Directory.Exists(AppDir)) Directory.CreateDirectory(AppDir);
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