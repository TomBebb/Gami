using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gami.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace Gami.Db.Models;

public sealed class Game : ViewModelBase, IGameLibraryRef
{
    public Game()
    {
    }

    public Game(IGameLibraryRef libraryRef)
    {
        LibraryType = libraryRef.LibraryType;
        LibraryId = libraryRef.LibraryId;
    }

    [Key] public int Id { get; set; }

    [Reactive] public GameInstallStatus InstallStatus { get; set; }
    public bool Installed => InstallStatus == GameInstallStatus.Installed;
    public DateTime ReleaseDate { get; set; }
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

    public string Name { get; set; } = null!;

    public string LibraryType { get; set; } = "";
    public string LibraryId { get; set; } = "";
}