namespace Gami.Core.Models;

public sealed class GameAgeRating
{
    public string GameId { get; set; } = null!;
    public int AgeRatingId { get; set; }
    public Game Game { get; set; } = null!;
    public AgeRating AgeRating { get; set; } = null!;
}