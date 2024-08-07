using Gami.Core.Models;

namespace Gami.Core;

public interface IGameLibraryScanner
{
    public string Type { get; }
    public IAsyncEnumerable<IGameLibraryRef> Scan();
}