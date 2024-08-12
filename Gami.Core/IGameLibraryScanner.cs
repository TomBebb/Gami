using Gami.Core.Models;

namespace Gami.Core;

public interface IGameLibraryScanner : IBasePlugin
{
    public IAsyncEnumerable<IGameLibraryMetadata> Scan();
}