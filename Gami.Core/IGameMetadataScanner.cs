using Gami.Core.Models;

namespace Gami.Core;

public interface IGameMetadataScanner : IBasePlugin
{
    public ValueTask<GameMetadata> ScanMetadata(IGameLibraryRef game);
}