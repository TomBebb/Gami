namespace Gami.Db.Schema.Metadata;

public sealed class GameRegion
{
    public int GameId { get; set; }
    public int RegionId { get; set; }
    public Game Game { get; set; }
    public Region Region { get; set; }
}