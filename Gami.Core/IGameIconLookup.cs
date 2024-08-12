using Gami.Core.Models;

namespace Gami.Core;

public interface IGameIconLookup : IBasePlugin
{
    public ValueTask<byte[]?> LookupIcon(IGameLibraryRef id);
}