using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SixLabors.ImageSharp;

namespace Gami.Desktop;

public static class CompressImageUtils
{
    public static async ValueTask<byte[]> CompressImage(this byte[] bytes,
        CancellationToken cancellationToken)
    {
        var currUnlockedIconBytes = bytes.Length;

        var unlockedImage = Image.Load(bytes);
        var unlockedImageStream = new MemoryStream();
        await unlockedImage.SaveAsWebpAsync(unlockedImageStream,
            cancellationToken);

        var webpBytes = unlockedImageStream.GetBuffer();


        Log.Debug("Optimised image; {Before} => {After}",
            currUnlockedIconBytes, webpBytes.Length);

        return webpBytes;
    }
}