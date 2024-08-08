namespace Gami.Core.Models;

public sealed class GameRegion
{
    public string GameId { get; set; }  = null!;
    public int RegionId { get; set; }
    public Game Game { get; set; } = null!;
    public Region Region { get; set; } = null!;
}