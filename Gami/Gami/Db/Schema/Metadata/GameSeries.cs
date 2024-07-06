namespace Gami.Db.Schema.Metadata;

public sealed class GameSeries
{
    public int GameId { get; set; }
    public int SeriesId { get; set; }
    public Game Game { get; set; }
    public Series Series { get; set; }
}