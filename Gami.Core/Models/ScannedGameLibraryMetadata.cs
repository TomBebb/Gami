namespace Gami.Core.Models;

public class ScannedGameLibraryMetadata : GameLibraryRef, IGameLibraryMetadata
{
    public GameInstallStatus InstallStatus { get; set; }
    public TimeSpan Playtime { get; set; }
}