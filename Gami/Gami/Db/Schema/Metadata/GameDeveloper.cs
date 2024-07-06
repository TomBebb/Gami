namespace Gami.Db.Schema.Metadata;

public sealed class GameDeveloper
{
    public int GameId { get; set; }
    public int DeveloperId { get; set; }
    public Game Game { get; set; }
    public Developer Developer { get; set; }
}