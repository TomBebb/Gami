namespace Gami.Core.Models;

public sealed class GameSeries
{
    public string GameId { get; set; } = null!;
    public int SeriesId { get; set; }
    public Game Game { get; set; } = null!;
    public Series Series { get; set; } = null!;
}