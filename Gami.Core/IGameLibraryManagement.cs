using Gami.Core.Models;

namespace Gami.Core;

public interface IGameLibraryManagement : IBasePlugin
{
    public ValueTask Install(IGameLibraryRef game);
    public void Uninstall(IGameLibraryRef game);

    public ValueTask<GameInstallStatus> CheckInstallStatus(IGameLibraryRef game);
}