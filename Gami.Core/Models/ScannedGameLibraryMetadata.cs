namespace Gami.Core.Models;

public class ScannedGameLibraryMetadata : GameLibraryRef, IGameLibraryMetadata
{
    public DateTime? LastPlayed { get; set; }
    public GameInstallStatus InstallStatus { get; set; }
    public TimeSpan Playtime { get; set; }
    public Uri? IconUrl { get; set; }
    public Uri? HeaderUrl { get; set; }
    public Uri? HeroUrl { get; set; }
    public Uri? LogoUrl { get; set; }

}