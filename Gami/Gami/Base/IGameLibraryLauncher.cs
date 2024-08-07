using System.Threading.Tasks;

namespace Gami.Base;

public interface IGameLibraryLauncher
{
    public string Type { get; }
    public void Launch(string id);
}