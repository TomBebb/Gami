namespace Gami.Core.Models;

public sealed class GameGenre
{
    public string GameId { get; set; }
    public int GenreId { get; set; }
    public Game Game { get; set; }
    public Genre Genre { get; set; }
}