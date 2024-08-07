namespace Gami.Core.Models;

public sealed class GamePlatform
{
    public int GameId { get; set; }
    public int PlatformId { get; set; }
    public Game Game { get; set; }
    public Platform Platform { get; set; }
}