using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DynamicData.Binding;
using Gami.BigPicture.Inputs;
using Gami.Core.Models;
using Gami.LauncherShared.Db;
using Gami.LauncherShared.Misc;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace Gami.BigPicture.ViewModels;

public sealed record MappedGame(Game Game, bool Selected = false);

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
                if (SelectedColumn > 0 && InputManager.ActiveInputs.Contains(MappedInputType.Left))
                    SelectedColumn--;
                if (SelectedColumn + 1 <= TilesPerRow && InputManager.ActiveInputs.Contains(MappedInputType.Right))
                    SelectedColumn++;
                if (SelectedRow > 0 && InputManager.ActiveInputs.Contains(MappedInputType.Up))
                    SelectedRow--;
                if (SelectedRow + 1 <= TotalRows && InputManager.ActiveInputs.Contains(MappedInputType.Down))
                    SelectedRow++;
                await Task.Delay(200);
            }
        });
        this.WhenAnyValue(v => v.SelectedGame, v => v.Games)
            .Subscribe(v =>
            {
                Log.Information($"Game: {v.Item1?.Name}");
                MappedGames = [..v.Item2.Select(g => new MappedGame(g, g == v.Item1))];
                Log.Debug("MappedGames: {Games}",
                    JsonSerializer.Serialize(MappedGames.Select(g => new { g.Game.Name, g.Selected })));
            });
        this.WhenAnyValue(v => v.SelectedRow, v => v.SelectedColumn)
            .Where(_ => Games.Length > 0)
            .Subscribe(v =>
            {
                var row = v.Item1;
                var column = v.Item2;
                Log.Debug("Game pos changed: {Row}; {Column}", row, column);
                SelectedGame = Games[row * TilesPerRow + column];
            });
    }

    [Reactive] public int TilesPerRow { get; set; } = 6;
    public int TotalRows => Games.Length / TilesPerRow;

    [Reactive] public int SortFieldIndex { get; set; }
    [Reactive] public string Search { get; set; } = "";
    [Reactive] public Game? SelectedGame { get; set; }
    [Reactive] public ImmutableArray<Game> Games { get; private set; } = ImmutableArray<Game>.Empty;

    [Reactive] public ImmutableArray<MappedGame> MappedGames { get; set; } = ImmutableArray<MappedGame>.Empty;

    [Reactive] public InputManager InputManager { get; set; } = new();
    [Reactive] public int SelectedRow { get; set; }
    [Reactive] public int SelectedColumn { get; set; }

    private void RefreshCache()
    {
        var sort = (SortGameField)SortFieldIndex;
        Log.Debug("Refresh cache - Search: {Search}; Sort: {Sort}", Search, sort);
        const SortDirection dir = SortDirection.Ascending;

        using var db = new GamiContext();
        var games = db.Games.Where(v => string.IsNullOrEmpty(Search) || EF.Functions.Like(v.Name, $"%{Search}%"));

        Games = games.Select(g => new Game
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
            .ToImmutableArray();

        SelectedGame = games.FirstOrDefault();
    }
}