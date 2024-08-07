namespace Gami.Core.Models;

public sealed class GameSeries
{
    public string GameId { get; set; }
    public int SeriesId { get; set; }
    public Game Game { get; set; }
    public Series Series { get; set; }
}