using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Gami.Core;
using Gami.Desktop.Db;
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

                DbOps.DoScan().GetAwaiter().GetResult();
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