using System.Collections.Immutable;

namespace Gami.Core.Models;

public sealed class Publisher : NamedIdItem
{
    public List<Game> Games { get; set; } = null!;
}