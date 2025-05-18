using System.Diagnostics;
using Gami.Core.Models;

namespace Gami.Core;

public interface IGameLibraryLauncher : IBaseAddon
{
    public void Launch(IGameLibraryRef gameRef);

    public ValueTask<Process?> GetMatchingProcess(IGameLibraryRef gameRef);
}