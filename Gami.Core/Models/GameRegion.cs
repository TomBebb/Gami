namespace Gami.Core.Models;

public sealed class GameRegion
{
    public string GameId { get; set; }
    public int RegionId { get; set; }
    public Game Game { get; set; }
    public Region Region { get; set; }
}