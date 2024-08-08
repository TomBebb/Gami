namespace Gami.Core.Models;

public sealed class AchievementProgress
{
    public string AchievementId { get; set; } = null!;
    public Achievement Achievement { get; set; } = null!;
    public bool Unlocked { get; set; } = false;
    public DateTime? UnlockTime { get; set; }
}