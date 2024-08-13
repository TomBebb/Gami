using System.Collections.Immutable;

namespace Gami.Library.Gog.Models;

public sealed class OwnedGames
{
    public ImmutableArray<long> Owned { get; set; }
}