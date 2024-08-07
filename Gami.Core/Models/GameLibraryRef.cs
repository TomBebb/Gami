namespace Gami.Core.Models;

public sealed class GameLibraryRef : IGameLibraryRef
{
    public string Name { get; set; } = null!;
    public string LibraryType { get; set; } = null!;
    public string LibraryId { get; set; } = null!;
    public GameInstallStatus InstallStatus { get; set; }
}