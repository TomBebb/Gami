using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Gami.Desktop.Conv;

public sealed class PercentValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return System.Convert.ToInt32(value).ToString().PadLeft(2, '0');
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}