using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Gami.LauncherShared.Models.Settings;

public sealed class AchievementsSettings : ReactiveObject
{
    [Reactive] public bool ShowAchievements { get; set; } = true;
    [Reactive] public bool FetchAchievements { get; set; } = true;
    [Reactive] public bool ScanAchievementsProgressOnStart { get; set; } = true;
    [Reactive] public bool OnlyScanInstalled { get; set; } = true;
}