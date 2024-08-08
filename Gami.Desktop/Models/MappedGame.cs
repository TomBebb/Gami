using System.IO;
using Avalonia.Media.Imaging;
using Gami.Core.Models;

namespace Gami.Desktop.Models;

public sealed class MappedGame : Game
{
    public Bitmap? IconBitmap => Icon != null ? new Bitmap(new MemoryStream(Icon)) : null;
}