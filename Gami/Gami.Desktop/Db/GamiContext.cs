using System.IO;
using Gami.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Gami.Desktop.Db;

public class GamiContext : DbContext
{
    public static readonly string DbPath = Path.Join(App.AppDir, "gami.db");
    public DbSet<Achievement> Achievements { get; set; }
    public DbSet<GameAchievement> GameAchievements { get; set; }
    public DbSet<AgeRating> AgeRatings { get; set; }
    public DbSet<GameAgeRating> GameAgeRatings { get; set; }
    public DbSet<Developer> Developers { get; set; }
    public DbSet<GameDeveloper> GameDevelopers { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<GameFeature> GameFeatures { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<GameGenre> GameGenres { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<Platform> Platforms { get; set; }
    public DbSet<GamePlatform> GamePlatforms { get; set; }
    public DbSet<Publisher> Publishers { get; set; }
    public DbSet<GamePublisher> GamePublishers { get; set; }
    public DbSet<Region> Regions { get; set; }
    public DbSet<GameRegion> GameRegions { get; set; }
    public DbSet<Series> Series { get; set; }
    public DbSet<GameSeries> GameSeries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Game>()
            .HasIndex(u => new { u.LibraryType, u.LibraryId })
            .IsUnique();
        builder.Entity<GameAchievement>()
            .HasKey(u => new { u.GameId, u.AchievementId });
        builder.Entity<GameAgeRating>()
            .HasKey(u => new { u.GameId, u.AgeRatingId });
        builder.Entity<GameFeature>()
            .HasKey(u => new { u.GameId, u.FeatureId });
        builder.Entity<GameDeveloper>()
            .HasKey(u => new { u.GameId, u.DeveloperId });
        builder.Entity<GameGenre>()
            .HasKey(u => new { u.GameId, u.GenreId });
        builder.Entity<GamePlatform>()
            .HasKey(u => new { u.GameId, u.PlatformId });
        builder.Entity<GamePublisher>()
            .HasKey(u => new { u.GameId, u.PublisherId });
        builder.Entity<GameRegion>()
            .HasKey(u => new { u.GameId, u.RegionId });
        builder.Entity<GameSeries>()
            .HasKey(u => new { u.GameId, u.SeriesId });
    }
}