﻿using Gami.Db.Schema.Metadata;
using Microsoft.EntityFrameworkCore;

namespace Gami.Db;

public class GamiContext : DbContext
{
    public DbSet<Achievement> Achievements { get; set; }
    public DbSet<AgeRating> AgeRatings { get; set; }
    public DbSet<Developer> Developers { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<Platform> Platforms { get; set; }
    public DbSet<Publisher> Publishers { get; set; }
    public DbSet<Region> Regions { get; set; }
    public DbSet<Series> Series { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=:memory:");
    }
}