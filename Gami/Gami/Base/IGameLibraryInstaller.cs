using System.Threading.Tasks;

namespace Gami.Base;

public interface IGameLibraryInstaller
{
    public string Type { get; }
    public void Install(string id);
}