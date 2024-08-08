namespace Gami.Core.Models;

public sealed class GamePublisher
{
    public string GameId { get; set; } = null!;
    public int PublisherId { get; set; }
    public Game Game { get; set; } = null!;
    public Publisher Publisher { get; set; } = null!;
}