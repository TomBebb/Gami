namespace Gami.Core.Models;

public sealed class GamePublisher
{
    public int GameId { get; set; }
    public int PublisherId { get; set; }
    public Game Game { get; set; }
    public Publisher Publisher { get; set; }
}