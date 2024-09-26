using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AvaloniaWebView;
using Gami.Core;
using Gami.Desktop.Db;
using Gami.Desktop.MIsc;
using Gami.Desktop.ViewModels;
using Gami.Desktop.Views;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using Serilog;

namespace Gami.Desktop;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void RegisterServices()
    {
        base.RegisterServices();

        // if you use only WebView  
        AvaloniaWebViewBuilder.Initialize(default);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
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
                break;
            }
            case ISingleViewApplicationLifetime singleViewPlatform:
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = new LibraryViewModel()
                };
                break;
        }

        base.OnFrameworkInitializationCompleted();

        var window = WindowUtil.GetMainWindow();
        Log.Information("App Window: {window}", window);

        var settings = new SettingsViewModel();

        settings.Watch();
        Log.Information("Got settings: {DAta}",
            JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        var trayIcon = new TrayIcon
        {
            IsVisible = settings.Settings.ShowSystemTrayIcon,
            ToolTipText = "Gami",
            Icon = new WindowIcon(new Bitmap("C:\\Users\\topha\\Code\\Gami\\Gami.Desktop\\Assets\\avalonia-logo.ico")),
            Menu =
            [
                new NativeMenuItem("Open Gami") { Command = ReactiveCommand.Create(OpenOnClick) },
                new NativeMenuItem("Exit Gami") { Command = ReactiveCommand.Create(Close) }
            ]
        };
        SettingsViewModel.SettingsChanged += settings =>
        {
            Log.Information("SettingsChanged");
            Dispatcher.UIThread.Post(() =>
                trayIcon.IsVisible = settings.ShowSystemTrayIcon);
        };
        return;
        var trayIcons = new TrayIcons
        {
            trayIcon
        };
        SetValue(TrayIcon.IconsProperty, settings.Settings.ShowSystemTrayIcon ? trayIcons : null);
    }

    private void OpenOnClick()
    {
        Log.Debug("Opening on click");
        var window = WindowUtil.GetMainWindow()!;
        window.WindowState = WindowState.Normal;
        window.Show();

        //window.Activate();
    }

    private void Close()
    {
        WindowUtil.GetMainWindow()?.Close();
    }
}