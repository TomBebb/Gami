using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Humanizer;

namespace Gami.Desktop.Conv;

public sealed class HumanizerConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter,
        CultureInfo culture) =>
        value?.ToString().Humanize();

    public object ConvertBack(object? value, Type targetType,
        object? parameter, CultureInfo culture) =>
        value?.ToString().Dehumanize();
}