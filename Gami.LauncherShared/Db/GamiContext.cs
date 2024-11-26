using System.Diagnostics.CodeAnalysis;
using Gami.Core;
using Gami.Core.Models;
using Gami.LauncherShared.Db.Models;
using LiteDB;
using LiteDB.Async;
using Microsoft.EntityFrameworkCore;

namespace Gami.LauncherShared.Db;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class GamiContext : LiteDatabaseAsync
{
    private static readonly string DbPath = Path.Join(Consts.AppDir, "gami.db");
    public ILiteCollectionAsync<Achievement> Achievements => GetCollection<Achievement>();
    public ILiteCollectionAsync<AchievementProgress> AchievementsProgresses => GetCollection<AchievementProgress>();
    public ILiteCollectionAsync<AgeRating> AgeRatings => GetCollection<AgeRating>();
    public ILiteCollectionAsync<Developer> Developers => GetCollection<Developer>();
    public ILiteCollectionAsync<Feature> Features => GetCollection<Feature>();
    public ILiteCollectionAsync<Genre> Genres => GetCollection<Genre>();
    public ILiteCollectionAsync<Game> Games => GetCollection<Game>();
    public ILiteCollectionAsync<Platform> Platforms => GetCollection<Platform>();
    public ILiteCollectionAsync<Publisher> Publishers => GetCollection<Publisher>();
    public ILiteCollectionAsync<Series> Series => GetCollection<Series>();
    public ILiteCollectionAsync<ExcludedGame> ExcludedGames => GetCollection<ExcludedGame>();

    public GamiContext() : base($"Filename=${DbPath};Connection=shared")
    {
    }

    private async ValueTask InitDbAsync()
    {
        await Games.EnsureIndexAsync(v => new { v.LibraryType, v.LibraryId });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Achievement>()
            .Property(g => g.GameId)
            .HasComputedColumnSql("substr([Id], 0, instr([Id], '::'))");

        builder.Entity<Achievement>()
            .HasKey(e => e.Id);
        builder.Entity<AchievementProgress>()
            .HasKey(e => e.AchievementId);

        builder.Entity<Achievement>()
            .HasOne(e => e.Progress)
            .WithOne(e => e.Achievement)
            .HasForeignKey<AchievementProgress>(e => e.AchievementId);

        builder.Entity<Achievement>()
            .Property(g => g.LibraryId)
            .HasComputedColumnSql("substr([Id], instr(id, '::')+1)");

        builder.Entity<Game>()
            .Property(g => g.LibraryType)
            .HasComputedColumnSql("substr([Id], 0, instr([Id], ':'))");

        builder.Entity<Game>()
            .Property(g => g.LibraryId)
            .HasComputedColumnSql("substr([Id], instr([id], ':')+1)");

        builder.Entity<ExcludedGame>()
            .HasKey(e => new { e.LibraryId, e.LibraryType });
    }
}