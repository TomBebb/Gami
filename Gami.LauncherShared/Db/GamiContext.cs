﻿using System.Diagnostics.CodeAnalysis;
using Gami.Core;
using Gami.Core.Models;
using Gami.LauncherShared.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace Gami.LauncherShared.Db;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class GamiContext : DbContext
{
    private static readonly string DbPath = Path.Join(Consts.AppDir, "gami.db");
    public DbSet<Achievement> Achievements { get; set; }
    public DbSet<AchievementProgress> AchievementsProgresses { get; set; }
    public DbSet<AgeRating> AgeRatings { get; set; }
    public DbSet<Developer> Developers { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<Platform> Platforms { get; set; }
    public DbSet<Publisher> Publishers { get; set; }
    public DbSet<Series> Series { get; set; }
    public DbSet<ExcludedGame> ExcludedGames { get; set; }

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