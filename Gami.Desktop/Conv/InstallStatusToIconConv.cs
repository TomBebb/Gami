using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using Gami.Core.Models;

namespace Gami.Desktop.Conv;

public class InstallStatusToIconConv : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!(value is GameInstallStatus status))
            throw new ArgumentException(nameof(value));

        return status switch
        {
            GameInstallStatus.Installing => Symbol.Rotate,
            GameInstallStatus.Queued => Symbol.Rotate,
            GameInstallStatus.Installed => Symbol.Checkmark,
            GameInstallStatus.InLibrary => Symbol.Clear
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}