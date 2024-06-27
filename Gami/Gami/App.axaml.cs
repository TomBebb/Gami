using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Gami.Db;
using Gami.ViewModels;
using Gami.Views;
using Microsoft.EntityFrameworkCore;

namespace Gami;

public class App : Application
{
    public static readonly string AppDir =
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "gami");


    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
            if (!Design.IsDesignMode)
            {
                if (!Directory.Exists(AppDir))
                    Directory.CreateDirectory(AppDir);
                using DbContext context = new GamiContext();
                Console.WriteLine("Ensure DB created");
                context.Database.EnsureCreated();
                Console.WriteLine("Save changes");
                context.SaveChanges();
                Console.WriteLine("Saved changes");
            }
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