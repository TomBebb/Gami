using Gami.Core.Models;

namespace Gami.Core;

public interface IGameMetadataScanner : IBaseAddon
{
    public ValueTask<GameMetadata> ScanMetadata(IGameLibraryRef game);
}