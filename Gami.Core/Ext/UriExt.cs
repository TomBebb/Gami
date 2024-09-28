namespace Gami.Core.Ext;

public static class UriExt
{
    public static async ValueTask<Uri?> AutoDownloadUriOpt(this Uri? uri, Lazy<string> name)
    {
        if (uri == null)
            return null;
        return await AutoDownloadUri(uri, name);
    }

    public static async ValueTask<Uri> AutoDownloadUriOpt(this string uri, Lazy<string> name)
    {
        return await AutoDownloadUri(new Uri(uri), name);
    }

    public static async ValueTask<Uri> AutoDownloadUri(this Uri uri, Lazy<string> name)
    {
        if (uri.IsFile) return uri;

        Directory.CreateDirectory(Consts.ImagesDir);

        var uriPath = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
        var ext = Path.GetExtension(uriPath);
        var destPath = Path.Join(Consts.ImagesDir, $"{name.Value}.webp");

        using var client = new HttpClient();
        var stream = await client.GetStreamAsync(uri);
        var outStream = File.Create(destPath);
        if (ext == ".webp")
            await stream.CopyToAsync(outStream);
        else
            await stream.CompressWebp(outStream);

        return new Uri(destPath);
    }
}