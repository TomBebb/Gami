using Gami.Core.Models;

namespace Gami.Core;

public interface IGameIconLookup : IBaseAddon
{
    public ValueTask<Uri?> LookupIcon(IGameLibraryRef id);
}