using System;
using System.Collections.Immutable;
using System.Linq;
using Gami.Core.Models;
using Gami.Desktop.Db;
using Gami.Desktop.Models;
using Humanizer;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace Gami.Desktop.ViewModels;

public class AchievementsViewModel : ViewModelBase
{
    private Game? _selectedGame;

    public AchievementsViewModel()
    {
        using (var db = new GamiContext())
        {
            Games = [..db.Games.Select(g => new Game { Id = g.Id, Name = g.Name, LibraryId = g.LibraryId })];
            SelectedGame = Games.FirstOrDefault();
        }

        this.WhenAnyValue(v => v.Filter).Subscribe(v => { ReloadAchievements(); });
    }

    [Reactive] public ImmutableArray<Game> Games { get; set; }

    [Reactive] public AchievementsFilter Filter { get; set; }

    public string[] FilterOptions => Enum.GetValues<AchievementsFilter>().Select(af => af.Humanize()).ToArray();

    public Game? SelectedGame
    {
        get => _selectedGame;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedGame, value);
            ReloadAchievements();
        }
    }

    [Reactive]
    public ImmutableArray<AchievementData> Achievements { get; set; } = ImmutableArray<AchievementData>.Empty;

    private void ReloadAchievements()
    {
        Log.Debug("Selected game changed! Fetching achievements");
        using var db = new GamiContext();

        Achievements = db.Achievements.AsQueryable().Where(a => a.GameId == SelectedGame.Id)
            .Where(a => Filter == AchievementsFilter.None ||
                        Filter == AchievementsFilter.Locked == !a.Progress.Unlocked)
            .Select(a => new AchievementData(a, a.Progress ?? new AchievementProgress()))
            .ToImmutableArray();
        Log.Debug("Selected game changed! Fetched achievements");
    }
}