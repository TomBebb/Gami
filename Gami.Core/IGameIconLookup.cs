using Gami.Core.Models;

namespace Gami.Core;

public interface IGameIconLookup
{
    public string Type { get; }
    public ValueTask<byte[]?> LookupIcon(IGameLibraryRef id);
}