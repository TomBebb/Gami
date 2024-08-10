using Gami.Core.Models;

namespace Gami.Core;

public interface IGameMetadataScanner
{
    public string Type { get; }

    public ValueTask<GameMetadata> ScanMetadata(IGameLibraryRef game);
}