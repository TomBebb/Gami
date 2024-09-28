using Gami.Core.Models;

namespace Gami.Core;

public interface IGameIconLookup : IBasePlugin
{
    public ValueTask<Uri?> LookupIcon(IGameLibraryRef id);
}