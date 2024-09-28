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
    private Game? _selectedGame;

    public AchievementsViewModel()
    {
        using (var db = new GamiContext())
        {
            Games = [..db.Games.Select(g => new Game { Id = g.Id, Name = g.Name, LibraryId = g.LibraryId })];
            SelectedGame = Games.FirstOrDefault();
        }
    }

    [Reactive] public ImmutableArray<Game> Games { get; set; }

    public Game? SelectedGame
    {
        get => _selectedGame;
        set
        {
            Log.Debug("Selected game changed! Fethcing achievements");
            using var db = new GamiContext();

            Achievements = db.Achievements.AsQueryable().Where(a => a.GameId == value.Id)
                .Select(a => new AchievementData(a, new AchievementProgress()))
                .ToImmutableArray();
            Log.Debug("Selected game changed! Fetched achievements");

            this.RaiseAndSetIfChanged(ref _selectedGame, value);
        }
    }

    [Reactive]
    public ImmutableArray<AchievementData> Achievements { get; set; } = ImmutableArray<AchievementData>.Empty;
}