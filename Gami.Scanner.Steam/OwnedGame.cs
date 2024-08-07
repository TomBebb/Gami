using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Gami.Scanner.Steam;

public sealed class OwnedGame
{
    [JsonPropertyName("appid")] public long AppId { get; set; }
    public string Name { get; set; }

    public long PlaytimeForever { get; set; }

    public string ImgIconUrl { get; set; }
}

public sealed class OwnedGamesGames
{
    public ImmutableArray<OwnedGame> Games { get; set; }
}

public sealed class OwnedGamesResults
{
    public OwnedGamesGames Response { get; set; }
}