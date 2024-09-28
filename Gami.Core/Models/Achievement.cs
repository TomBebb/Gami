using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Gami.Core.Models;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public sealed class Achievement
{
    [Key] public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string GameId { get; set; } = null!;
    public string LibraryId { get; set; } = null!;
    public string LockedIconUrl { get; set; } = null!;
    public string UnlockedIconUrl { get; set; } = null!;
    public AchievementProgress? Progress { get; set; }

    public List<Game> Games { get; set; } = null!;

    public float? GlobalPercent { get; set; }
}