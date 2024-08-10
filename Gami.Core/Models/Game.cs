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
    [Reactive] public string Description { get; set; } = null!;

    public byte[]? Icon { get; set; }

    public List<Achievement> Achievements { get; set; } = null!;

    public List<AgeRating> AgeRatings { get; set; } = null!;
    public List<Developer> Developers { get; set; } = null!;
    public List<Feature> Features { get; set; } = null!;
    public List<Genre> Genres { get; set; } = null!;
    public List<Platform> Platforms { get; set; } = null!;
    public List<Publisher> Publishers { get; set; } = null!;
    public List<Series> Series { get; set; } = null!;

    [Reactive] public GameInstallStatus InstallStatus { get; set; }
    [Reactive] public TimeSpan Playtime { get; set; }

    public string Name { get; set; } = null!;

    public string LibraryType { get; set; } = "";
    public string LibraryId { get; set; } = "";
}