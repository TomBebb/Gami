using System;
using System.Collections.Immutable;
using System.Linq;
using Gami.Core.Models;
using Gami.Desktop.Db;
using Gami.Desktop.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace Gami.Desktop.ViewModels;

public class AchievementsViewModel : ViewModelBase
{
    public AchievementsViewModel()
    {
        using (var db = new GamiContext())
        {
            Games = [..db.Games.Select(g => new Game { Id = g.Id, Name = g.Name, LibraryId = g.LibraryId })];
            SelectedGame = Games.FirstOrDefault();
        }

        this.WhenAnyValue(x => x.SelectedGame)
            .Subscribe(g =>
            {
                Log.Information("Selected game changed! Fethcing achievements");
                using var db = new GamiContext();

                Achievements = db.Achievements.AsQueryable().Where(a => a.GameId == g.Id)
                    .Select(a => new AchievementData(a, new AchievementProgress()))
                    .ToImmutableArray();
                Log.Information("Selected game changed! Fetched achievements");
            });
    }

    [Reactive] public ImmutableArray<Game> Games { get; set; }

    [Reactive] public Game? SelectedGame { get; set; }

    [Reactive]
    public ImmutableArray<AchievementData> Achievements { get; set; } = ImmutableArray<AchievementData>.Empty;
}