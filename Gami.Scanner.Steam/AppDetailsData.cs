using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Gami.Scanner.Steam;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public sealed class AppDetailsData
{
    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string DetailedDescription { get; set; } = null!;
    public string ShortDescription { get; set; } = null!;
    public string AboutTheGame { get; set; } = null!;
    public string? Website { get; set; }

    public ImmutableArray<string> Developers { get; set; }
    public ImmutableArray<string> Publishers { get; set; }

    public ImmutableArray<AppGenre>? Genres { get; set; }
}

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public sealed class AppGenre
{
    public string Id { get; set; } = null!;
    public string Description { get; set; } = null!;
}

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public sealed class AppDetails
{
    public bool Success { get; set; }
    public AppDetailsData Data { get; set; } = null!;
}