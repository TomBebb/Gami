using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using Gami.Core.Models;
using Gami.Desktop.Misc;
using Gami.Desktop.ViewModels;

namespace Gami.Desktop.Conv;

public sealed class MenuItemsConverter : IValueConverter
{
    private static readonly Lazy<MainViewModel> LazyMainViewModel = new(() =>
    {
        var window = WindowUtil.GetMainWindow();
        var main = window!.GetVisualChildren().First();
        return (MainViewModel)main.DataContext!;
    });

    public object? Convert(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        var libraryModel = LazyMainViewModel.Value.AsLibrary!;
        if (value is not Game game)
            throw new NotSupportedException(value?.GetType().Name);

        var vals = new List<MenuItem>(4);
        switch (game.InstallStatus)
        {
            case GameInstallStatus.Installed:
                vals.Add(new MenuItem
                {
                    Header = "Play",
                    Icon = new SymbolIcon { Symbol = Symbol.Play },
                    Command = libraryModel.PlayGame,
                    CommandParameter = game
                });
                vals.Add(new MenuItem
                {
                    Header = "Uninstall",
                    Icon = new SymbolIcon { Symbol = Symbol.Clear },
                    Command = libraryModel.UninstallGame,
                    CommandParameter = game
                });
                break;
            case GameInstallStatus.InLibrary:
                vals.Add(new MenuItem
                {
                    Header = "Install",
                    Icon = new SymbolIcon { Symbol = Symbol.Add },
                    Command = libraryModel.InstallGame,
                    CommandParameter = game
                });
                break;
            default:
                break;
        }

        vals.Add(new MenuItem
        {
            Header = "Edit",
            Icon = new SymbolIcon { Symbol = Symbol.Edit },
            Command = libraryModel.EditGame,
            CommandParameter = game
        });
        vals.Add(new MenuItem
        {
            Header = "Delete",
            Icon = new SymbolIcon { Symbol = Symbol.Delete },
            Command = libraryModel.DeleteGame,
            CommandParameter = game
        });
        return vals;
    }

    public object ConvertBack(object? value, Type targetType,
        object? parameter, CultureInfo culture)
    {
        throw new UnreachableException();
    }
}