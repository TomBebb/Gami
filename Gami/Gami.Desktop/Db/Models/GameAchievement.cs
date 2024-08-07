namespace Gami.Desktop.Db.Models;

public sealed class GameAchievement
{
    public int GameId { get; set; }
    public int AchievementId { get; set; }
    public Game Game { get; set; } = null!;
    public Achievement Achievement { get; set; } = null!;
}