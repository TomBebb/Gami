using Gami.Core.Models;

namespace Gami.Core;

public interface IGameLibraryManagement : IBasePlugin
{
    public void Install(string id);
    public void Uninstall(string id);

    public ValueTask<GameInstallStatus> CheckInstallStatus(string id);
}