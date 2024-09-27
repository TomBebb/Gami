using System.Collections.Immutable;
using System.Linq;
using Gami.Core.Models;
using Gami.Desktop.Db;
using Gami.Desktop.Models;
using ReactiveUI.Fody.Helpers;

namespace Gami.Desktop.ViewModels;

public class AchievementsViewModel : ViewModelBase
{
    public AchievementsViewModel()
    {
        using var db = new GamiContext();
        Games = [..db.Games.Select(g => new Game { Id = g.Id, Name = g.Name, LibraryId = g.LibraryId })];
        SelectedGame = Games.FirstOrDefault();
    }

    [Reactive] public ImmutableArray<Game> Games { get; set; } = ImmutableArray<Game>.Empty;

    [Reactive] public Game? SelectedGame { get; set; }

    public ImmutableArray<AchievementData> Achievements { get; set; } = ImmutableArray<AchievementData>.Empty;
}