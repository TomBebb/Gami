using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using Gami.Core.Models;

namespace Gami.Desktop.Conv;

public class InstallStatusToIconConv : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not GameInstallStatus status)
            throw new ArgumentException("Not " + nameof(GameInstallStatus), nameof(value));

        return status switch
        {
            GameInstallStatus.Installing => Symbol.Rotate,
            GameInstallStatus.Queued => Symbol.Rotate,
            GameInstallStatus.Installed => Symbol.Checkmark,
            GameInstallStatus.InLibrary => Symbol.Clear,
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}