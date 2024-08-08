namespace Gami.Core.Models;

public sealed class GameDeveloper
{
    public string GameId { get; set; } = null!;
    public int DeveloperId { get; set; }
    public Game Game { get; set; } = null!;
    public Developer Developer { get; set; } = null!;
}