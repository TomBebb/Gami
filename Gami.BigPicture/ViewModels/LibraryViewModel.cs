using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DynamicData.Binding;
using Gami.BigPicture.Inputs;
using Gami.Core.Models;
using Gami.LauncherShared.Db;
using Gami.LauncherShared.Misc;
using Microsoft.EntityFrameworkCore;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace Gami.BigPicture.ViewModels;

public class LibraryViewModel : ViewModelBase
{
    public LibraryViewModel()
    {
        Log.Information("LibraryViewModel");
        RefreshCache();
        Task.Run(async () =>
        {
            while (true)
            {
                if (InputManager.ActiveInputs.Contains(MappedInputType.Left))
                    SelectedColumn--;
                await Task.Delay(200);
            }
        });
    }

    [Reactive] public int TilesPerRow { get; set; } = 6;
    public int TotalRows => Games.Count / TilesPerRow;

    [Reactive] public int SortFieldIndex { get; set; }
    [Reactive] public string Search { get; set; } = "";
    [Reactive] public Game? SelectedGame { get; set; }
    [Reactive] public ImmutableList<Game> Games { get; private set; } = ImmutableList<Game>.Empty;

    [Reactive] public InputManager InputManager { get; set; } = new();
    [Reactive] public int SelectedRow { get; set; } = 0;
    [Reactive] public int SelectedColumn { get; set; }

    private void RefreshCache()
    {
        var sort = (SortGameField)SortFieldIndex;
        Log.Debug("Refresh cache - Search: {Search}; Sort: {Sort}", Search, sort);
        const SortDirection dir = SortDirection.Ascending;
        using var db = new GamiContext();
        var games = db.Games
            .Where(v => string.IsNullOrEmpty(Search) || EF.Functions.Like(v.Name, $"%{Search}%"));

        Games = games
            .Select(g => new Game
            {
                Name = g.Name,
                Description = g.Description,
                ReleaseDate = g.ReleaseDate,
                LastPlayed = g.LastPlayed,

                Playtime = g.Playtime,
                HeaderUrl = g.HeaderUrl,
                IconUrl = g.IconUrl,
                HeroUrl = g.HeroUrl,
                LogoUrl = g.LogoUrl,
                Id = g.Id,
                Publishers = g.Publishers.Select(v => new Publisher { Name = v.Name }).ToList(),
                Developers = g.Developers.Select(v => new Developer { Name = v.Name }).ToList(),
                Genres = g.Genres.Select(v => new Genre { Name = v.Name }).ToList(),
                LibraryType = g.LibraryType,
                LibraryId = g.LibraryId,
                InstallStatus = g.InstallStatus
            })
            .ToImmutableList();
    }
}