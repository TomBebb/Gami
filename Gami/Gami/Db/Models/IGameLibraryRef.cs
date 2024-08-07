namespace Gami.Db.Models;

public interface IGameLibraryRef
{
    public string Name { get; set; }
    public string LibraryType { get; set; }
    public string LibraryId { get; set; }

    public GameInstallStatus InstallStatus { get; set; }
}