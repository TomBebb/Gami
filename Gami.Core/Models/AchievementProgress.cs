using System.Diagnostics.CodeAnalysis;

namespace Gami.Core.Models;

[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public sealed class AchievementProgress
{
    public string AchievementId { get; set; } = null!;
    public Achievement Achievement { get; set; } = null!;
    public bool Unlocked { get; set; }
    public DateTime? UnlockTime { get; set; }
}