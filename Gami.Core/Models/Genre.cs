using System.Collections.Immutable;

namespace Gami.Core.Models;

public sealed class Genre : NamedIdItem
{
    public List<Game> Games { get; set; } = null!;
}