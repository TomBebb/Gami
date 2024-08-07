namespace Gami.Desktop.Base;

public interface IGameLibraryInstaller
{
    public string Type { get; }
    public void Install(string id);
}