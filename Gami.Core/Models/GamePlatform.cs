namespace Gami.Core.Models;

public sealed class GamePlatform
{
    public string GameId { get; set; } = null!;
    public int PlatformId { get; set; }
    public Game Game { get; set; } = null!;
    public Platform Platform { get; set; } = null!;
}