namespace Gami.Core.Models;

public sealed class GameFeature
{
    public int GameId { get; set; }
    public int FeatureId { get; set; }
    public Game Game { get; set; }
    public Feature Feature { get; set; }
}