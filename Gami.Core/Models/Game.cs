using System.ComponentModel.DataAnnotations;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Gami.Core.Models;

public sealed class Game : ReactiveObject, IGameLibraryRef
{
    [Key] public int Id { get; set; }
    public bool Installed => InstallStatus == GameInstallStatus.Installed;
    [Reactive] public DateTime ReleaseDate { get; set; }
    public string Description { get; set; } = null!;

    public List<GameAchievement> GameAchievements { get; } = null!;
    public List<GameAgeRating> GameAgeRatings { get; } = null!;
    public List<GameDeveloper> GameDevelopers { get; } = null!;
    public List<GameFeature> GameFeatures { get; }
    public List<GameGenre> GameGenres { get; set; } = null!;
    public List<GamePlatform> GamePlatforms { get; set; } = null!;
    public List<GamePublisher> GamePublishers { get; set; } = null!;
    public List<GameRegion> GameRegions { get; set; } = null!;
    public List<GameSeries> GameSeries { get; set; } = null!;

    [Reactive] public GameInstallStatus InstallStatus { get; set; }

    public string Name { get; set; } = null!;

    public string LibraryType { get; set; } = "";
    public string LibraryId { get; set; } = "";
}