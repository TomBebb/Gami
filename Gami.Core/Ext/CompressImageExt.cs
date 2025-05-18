using SixLabors.ImageSharp;

namespace Gami.Core.Ext;

public static class CompressImageExt
{
    public static async ValueTask<byte[]> CompressImage(this byte[] bytes,
        CancellationToken cancellationToken = default)
    {

        var unlockedImage = Image.Load(bytes);
        var unlockedImageStream = new MemoryStream();
        await unlockedImage.SaveAsWebpAsync(unlockedImageStream,
            cancellationToken);

        var webpBytes = unlockedImageStream.GetBuffer();

        return webpBytes;
    }

    public static async ValueTask CompressWebp(this Stream rawStream, Stream outStream,
        CancellationToken cancellationToken = default)
    {
        var unlockedImage = await Image.LoadAsync(rawStream, cancellationToken);
        await unlockedImage.SaveAsWebpAsync(outStream,
            cancellationToken);
    }
}