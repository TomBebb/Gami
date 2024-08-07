namespace Gami.Desktop.Db.Models;

public sealed class GameGenre
{
    public int GameId { get; set; }
    public int GenreId { get; set; }
    public Game Game { get; set; }
    public Genre Genre { get; set; }
}