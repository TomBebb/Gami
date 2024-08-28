using System;
using System.Collections.Immutable;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Gami.Desktop.Conv;

public sealed class TimeSpanDisplayConverter : IValueConverter
{
    public static readonly TimeSpanDisplayConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        if (value is not TimeSpan span)
            return new BindingNotification(new InvalidCastException(),
                BindingErrorType.Error);

        if (span <= TimeSpan.FromMinutes(1)) return "N/A";
        var parts = ImmutableList<string>.Empty;
        var totalHours = span.Hours + span.Days * 24;
        if (totalHours > 0) parts = parts.Add($"{totalHours}h");
        if (span.Minutes > 0) parts = parts.Add($"{span.Minutes}m");
        return string.Join(" ", parts);
    }

    public object ConvertBack(object? value, Type targetType,
        object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}