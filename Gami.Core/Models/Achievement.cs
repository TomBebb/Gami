using System.ComponentModel.DataAnnotations;

namespace Gami.Core.Models;

public sealed class Achievement
{
    [Key] public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string GameId { get; set; } = null!;
    public Game Game { get; set; } = null!;
    public string LibraryId { get; set; } = null!;
    public byte[] LockedIcon { get; set; } = null!;
    public byte[] UnlockedIcon { get; set; } = null!;
    public AchievementProgress? Progress { get; set; }
}