namespace Gami.Core.Models;

public sealed class GamePublisher
{
    public string GameId { get; set; }
    public int PublisherId { get; set; }
    public Game Game { get; set; }
    public Publisher Publisher { get; set; }
}