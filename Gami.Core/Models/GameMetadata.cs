using System.Collections.Immutable;

namespace Gami.Core.Models;

public record struct GameMetadata
{
    public string? Description { get; set; }

    public ImmutableArray<string>? Developers { get; set; }
    public ImmutableArray<string>? Genres { get; set; }
    public ImmutableArray<string>? Platforms { get; set; }
    public ImmutableArray<string>? Publishers { get; set; }
    public ImmutableArray<string>? Series { get; set; }

    public GameMetadata Combine(GameMetadata other) =>
        new()
        {
            Description = Description ?? other.Description,
            Developers = Developers ?? other.Developers,
            Genres = Genres ?? other.Genres,
            Platforms = Platforms ?? other.Platforms,
            Publishers = Publishers ?? other.Publishers,
            Series = Series ?? other.Series
        };
}