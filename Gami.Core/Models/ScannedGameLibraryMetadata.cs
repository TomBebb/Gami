namespace Gami.Core.Models;

public sealed class ScannedGameLibraryMetadata : GameLibraryRef, IGameLibraryMetadata
{
    public GameInstallStatus InstallStatus { get; set; }
    public byte[]? Icon { get; set; }
}