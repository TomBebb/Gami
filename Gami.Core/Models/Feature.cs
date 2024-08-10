using System.Collections.Immutable;

namespace Gami.Core.Models;

public sealed class Feature : NamedIdItem
{
    public List<Game> Games { get; set; } = null!;
}