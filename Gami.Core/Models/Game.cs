using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Gami.Core.Models;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public class Game : ReactiveObject, IGameLibraryMetadata
{
    [Key] public string Id { get; set; } = null!;
    public bool Installed => InstallStatus == GameInstallStatus.Installed;
    [Reactive] public DateTime ReleaseDate { get; set; }
    [Reactive] public string Description { get; set; } = null!;

    public Uri? IconUrl { get; set; }
    public Uri? HeaderUrl { get; set; }
    public Uri? HeroUrl { get; set; }
    public Uri? LogoUrl { get; set; }

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