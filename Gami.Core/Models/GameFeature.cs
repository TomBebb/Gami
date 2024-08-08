namespace Gami.Core.Models;

public sealed class GameFeature
{
    public string GameId { get; set; } = null!;
    public int FeatureId { get; set; }
    public Game Game { get; set; } = null!;
    public Feature Feature { get; set; } = null!;
}