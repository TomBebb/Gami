using Gami.Core.Models;

namespace Gami.Core;

public interface IGameLibraryScanner
{
    public IAsyncEnumerable<IGameLibraryRef> Scan();
}