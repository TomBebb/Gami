namespace Gami.Db.Schema.Metadata;

public sealed class GameAchievement
{
    public int GameId { get; set; }
    public int AchievementId { get; set; }
    public Game Game { get; set; }
    public Achievement Achievement { get; set; }
}