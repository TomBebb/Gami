using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SixLabors.ImageSharp;

namespace Gami.Core.Models;

public class Game : ReactiveObject, IGameLibraryRef
{
    [Key] public string Id { get; set; } = null!;
    public bool Installed => InstallStatus == GameInstallStatus.Installed;
    [Reactive] public DateTime ReleaseDate { get; set; }
    public string Description { get; set; } = null!;

    public byte[]? Icon { get; set; }

    [Reactive]
    public ImmutableList<Achievement> Achievements { get; set; } =
        ImmutableList<Achievement>.Empty;

    public List<GameAgeRating> GameAgeRatings { get; } = null!;
    public List<GameDeveloper> GameDevelopers { get; } = null!;
    public List<GameFeature> GameFeatures { get; } = null!;
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