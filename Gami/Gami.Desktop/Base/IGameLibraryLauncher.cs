namespace Gami.Desktop.Base;

public interface IGameLibraryLauncher
{
    public string Type { get; }
    public void Launch(string id);
}