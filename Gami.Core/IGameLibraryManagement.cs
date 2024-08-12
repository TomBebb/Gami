using Gami.Core.Models;

namespace Gami.Core;

public interface IGameLibraryManagement
{
    public string Type { get; }
    public void Install(string id);
    public void Uninstall(string id);

    public ValueTask<GameInstallStatus> CheckInstallStatus(string id);
}