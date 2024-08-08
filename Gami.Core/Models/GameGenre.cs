namespace Gami.Core.Models;

public sealed class GameGenre
{
    public string GameId { get; set; } = null!;
    public int GenreId { get; set; }
    public Game Game { get; set; } = null!;
    public Genre Genre { get; set; } = null!;
}