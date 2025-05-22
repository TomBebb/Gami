using System;
using System.Globalization;
using System.Net;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Gami.Desktop.Conv;

public class BitmapValueConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return null;
        if (targetType != typeof(IImage))
            throw new NotSupportedException();

        var uri = value is Uri val ? val : new Uri(value.ToString()!);
        var scheme = uri.IsAbsoluteUri ? uri.Scheme : "file";

        switch (scheme)
        {
            case "file":
                var mapped = WebUtility.UrlDecode(uri.AbsolutePath);
                return new Bitmap(mapped);

            default:
                return new Bitmap(AssetLoader.Open(uri));
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}