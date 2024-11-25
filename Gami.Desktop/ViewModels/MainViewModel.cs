using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Gami.Desktop.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace Gami.Desktop.ViewModels;

public sealed record Route(
    string Path,
    string Name,
    Func<string, ViewModelBase> ViewModelFactory,
    string Tooltip = "",
    Symbol? Icon = null);

public class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        CurrRoute = Routes.First();
        RouteDictionary = Routes.Concat(FooterRoutes).ToImmutableDictionary(v => v.Path);

        this.WhenAnyValue(v => v.CurrObject).Subscribe(curr =>
        {
            Log.Debug("CurrObject changed: {Curr}", curr);
            if (curr == null)
                return;

            if (curr is LibraryViewModel library)
                AsLibrary = library;
            else
                AsLibrary = null;

            CurrView = curr switch
            {
                LibraryViewModel lm => new LibraryView { DataContext = lm },
                SettingsViewModel sm => new SettingsView { DataContext = sm },
                AddonsViewModel am => new AddOnsView { DataContext = am },
                AchievementsViewModel atm => new AchievementsView { DataContext = atm },
                _ => throw new NotImplementedException()
            };
        });

        this.WhenAnyValue(v => v.CurrRoute).Subscribe(route =>
        {
            Log.Debug("Route changed: {Curr}", route);
            if (route == null)
                return;

            CurrObject = route.ViewModelFactory(route.Path);
        });
        this.WhenAnyValue(v => v.CurrPath).Subscribe(path =>
        {
            Log.Debug("Path changed: {Curr}", path);
            if (path == null)
                return;

            CurrRoute = RouteDictionary[path];
        });

        CurrPath = "library";

        CurrView = new LibraryView { DataContext = (LibraryViewModel)CurrObject! };
    }

    public List<Route> Routes { get; } =
    [
        new("library", "Library", _ => new LibraryViewModel(), "List installed games", Symbol.Library),
        new("achievements", "Achievements", _ => new AchievementsViewModel(), "List installed games", Symbol.Alert)
    ];

    public List<Route> FooterRoutes { get; } =
    [
        new("addons", "Add-ons", _ => new AddonsViewModel(), "Manage add-ons", Symbol.Admin),
        new("settings", "Settings", _ => new SettingsViewModel(), "List games", Symbol.Settings)
    ];

    private ImmutableDictionary<string, Route> RouteDictionary { get; }

    [Reactive] public string CurrPath { get; set; }
    [Reactive] public Route CurrRoute { get; set; }

    [Reactive] private ReactiveObject? CurrObject { get; set; }

    [Reactive] public UserControl? CurrView { get; set; }
    [Reactive] public LibraryViewModel? AsLibrary { get; set; }
}