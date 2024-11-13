using Gami.Core.Models;

namespace Gami.LauncherShared.Models;

public readonly record struct AchievementData(Achievement Achievement, AchievementProgress Progress)
{
    public string IconUrl => Progress.Unlocked ? Achievement.UnlockedIconUrl : Achievement.LockedIconUrl;
}