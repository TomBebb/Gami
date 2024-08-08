namespace Gami.Core.Models;

public interface IGameLibraryMetadata : IGameLibraryRef
{
    public GameInstallStatus InstallStatus { get; set; }
}