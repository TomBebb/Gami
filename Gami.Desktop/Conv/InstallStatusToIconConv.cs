using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Gami.Core.Models;
using Lucide.Avalonia;

namespace Gami.Desktop.Conv;

public class InstallStatusToIconConv : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not GameInstallStatus status)
            throw new ArgumentException("Not " + nameof(GameInstallStatus), nameof(value));

        return status switch
        {
            GameInstallStatus.Installing => LucideIconKind.RotateCcw,
            GameInstallStatus.Queued => LucideIconKind.RotateCcw,
            GameInstallStatus.Installed => LucideIconKind.Check,
            GameInstallStatus.InLibrary => LucideIconKind.X,
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}