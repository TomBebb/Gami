using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Markdig;

namespace Gami.Desktop.Conv;

public sealed class MarkdownHtmlConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        return value == null ? null : Markdown.ToHtml(value.ToString());
    }

    public object ConvertBack(object? value, Type targetType,
        object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}