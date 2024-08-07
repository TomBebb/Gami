namespace Gami.Core;

public interface IGameLibraryInstaller
{
    public string Type { get; }
    public void Install(string id);
}