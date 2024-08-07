namespace Gami.Core.Models;

public sealed class GameDeveloper
{
    public string GameId { get; set; }
    public int DeveloperId { get; set; }
    public Game Game { get; set; }
    public Developer Developer { get; set; }
}