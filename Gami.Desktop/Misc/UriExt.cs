using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Gami.Core;
using Serilog;

namespace Gami.Desktop.MIsc;

public static class UriExt
{
    public static async ValueTask<Uri?> AutoDownloadUriOpt(this Uri? uri, Lazy<string> name)
    {
        if (uri == null)
            return null;
        return await AutoDownloadUri(uri, name);
    }

    public static async ValueTask<Uri> AutoDownloadUri(this Uri uri, Lazy<string> name)
    {
        if (uri.IsFile)
        {
            return uri;
        }

        Directory.CreateDirectory(Consts.ImagesDir);

        var uriPath = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
        var ext = Path.GetExtension(uriPath);
        var destPath = Path.Join(Consts.ImagesDir, $"{name.Value}.{ext}");

        using var client = new HttpClient();
        var stream = await client.GetStreamAsync(uri);
        var outStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.Read);
        await stream.CopyToAsync(outStream);
        return new Uri(destPath);
    }
}