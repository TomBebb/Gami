using System.Diagnostics;
using Gami.Core.Models;

namespace Gami.Core;

public interface IGameLibraryLauncher
{
    public string Type { get; }
    public void Launch(string id);

    public ValueTask<Process?> GetMatchingProcess(IGameLibraryRef gameRef);
}