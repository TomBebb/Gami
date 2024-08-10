using System.Collections.Immutable;
using Gami.Core.Models;

namespace Gami.Scanner.Steam;

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

public sealed class AppGenre
{
    public string Id { get; set; }
    public string Description { get; set; }
}

public sealed class AppDetails
{
    public bool Success { get; set; }
    public AppDetailsData Data { get; set; }
}