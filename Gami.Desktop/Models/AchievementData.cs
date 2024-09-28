﻿using Gami.Core.Models;

namespace Gami.Desktop.Models;

public record struct AchievementData(Achievement Achievement, AchievementProgress Progress)
{
    public string IconUrl => Progress.Unlocked ? Achievement.UnlockedIconUrl : Achievement.LockedIconUrl;
}