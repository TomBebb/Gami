namespace Gami.Core.Models;

public interface IGameLibraryMetadata : IGameLibraryRef
{
    public GameInstallStatus InstallStatus { get; set; }
    public TimeSpan Playtime { get; set; }
}