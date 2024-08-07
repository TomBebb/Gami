namespace Gami.Core.Models;

public sealed class GameAgeRating
{
    public int GameId { get; set; }
    public int AgeRatingId { get; set; }
    public Game Game { get; set; }
    public AgeRating AgeRating { get; set; }
}