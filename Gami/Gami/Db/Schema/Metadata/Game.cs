using System;
using System.Collections.Generic;

namespace Gami.Db.Schema.Metadata;

public sealed class GameLibraryRef : IGameLibraryRef
{
    public string Name { get; set; } = null!;
    public string LibraryType { get; set; } = null!;
    public string LibraryId { get; set; } = null!;
}

public interface IGameLibraryRef
{
    public string Name { get; set; }
    public string LibraryType { get; set; }
    public string LibraryId { get; set; }
}

public sealed class Game : NamedIdItem, IGameLibraryRef
{
    public Game()
    {
    }

    public Game(IGameLibraryRef libraryRef)
    {
        LibraryType = libraryRef.LibraryType;
        LibraryId = libraryRef.LibraryId;
    }

    public DateTime ReleaseDate { get; set; }
    public string Description { get; set; } = null!;

    public List<Achievement> Achievements { get; set; } = null!;
    public List<AgeRating> AgeRatings { get; set; } = null!;
    public List<Developer> Developers { get; set; } = null!;
    public List<Feature> Features { get; set; } = null!;
    public List<Genre> Genres { get; set; } = null!;
    public List<Platform> Platforms { get; set; } = null!;
    public List<Publisher> Publishers { get; set; } = null!;
    public List<Region> Regions { get; set; } = null!;
    public List<Series> Series { get; set; } = null!;

    public string LibraryType { get; set; } = "";
    public string LibraryId { get; set; } = "";
}