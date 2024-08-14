using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Gami.Library.Gog.Models;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class GameDetails
{
    public string Title { get; set; } = null!;
    public string BackgroundImage { get; set; } = null!;
    public ImmutableArray<(string, ImmutableDictionary<string, ImmutableArray<Download>>)> Downloads { get; set; }
}