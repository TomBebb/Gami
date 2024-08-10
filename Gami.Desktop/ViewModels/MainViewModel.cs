using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using Gami.Core.Models;
using Gami.Desktop.Db;
using Gami.Desktop.Models;
using Gami.Desktop.Plugins;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace Gami.Desktop.ViewModels;

public class MainViewModel : ViewModelBase
{
    [Reactive] public string Search { get; set; }
    [Reactive] public MappedGame SelectedGame { get; set; } = null!;

    public MainViewModel()
    {
        PlayGame = ReactiveCommand.Create((Game game) =>
        {
            Log.Information("Play game: {Game}", JsonSerializer.Serialize(game));
            game.Launch();
        });
        InstallGame = ReactiveCommand.Create((Game game) =>
        {
            Log.Information("Install game: {Game}", JsonSerializer.Serialize(game));
            game.Install();
        });
        UninstallGame = ReactiveCommand.Create((Game game) =>
        {
            Log.Information("Uninstall game: {Game}", JsonSerializer.Serialize(game));
            game.Uninstall();
        });
        EditGame = ReactiveCommand.Create((MappedGame game) =>
        {
            Log.Information("Edit game: {Game}", JsonSerializer.Serialize(game));
            EditingGame = game;
        });
        Refresh = ReactiveCommand.Create(RefreshCache);
        ClearSearch = ReactiveCommand.Create(() => { Search = ""; });

        this.WhenAnyValue(v => v.Search).ForEachAsync((_) => RefreshCache());
        RefreshCache();
    }

    public void RefreshCache()
    {
        Log.Debug("Refresh cache - Search: {Search}", Search);
        using var db = new GamiContext();
        Games = db.Games
            .Where(v => string.IsNullOrEmpty(Search) || EF.Functions.Like(v.Name, $"%{Search}%"))
            .Select(v => new MappedGame()
            {
                LibraryType = v.LibraryType,
                LibraryId = v.LibraryId,
                InstallStatus = v.InstallStatus,
                Name = v.Name,
                GameGenres = v.GameGenres.Select(g => new GameGenre
                {
                    Genre = new Genre { Name = g.Genre.Name }
                }).ToImmutableList(),
                Description = v.Description,
                Icon = v.Icon,
                Playtime = v.Playtime
            }).ToImmutableList();
    }
#pragma warning disable CA1822 // Mark members as static

    [Reactive] public MappedGame? EditingGame { get; set; }

    public ReactiveCommand<Unit, Unit> ClearSearch { get; }
    public ReactiveCommand<Game, Unit> PlayGame { get; }
    public ReactiveCommand<MappedGame, Unit> EditGame { get; set; }
    public ReactiveCommand<Game, Unit> InstallGame { get; set; }
    public ReactiveCommand<Game, Unit> UninstallGame { get; set; }
    public ReactiveCommand<Unit, Unit> Refresh { get; set; }


    [Reactive] public ImmutableList<MappedGame> Games { get; private set; } = ImmutableList<MappedGame>.Empty;

#pragma warning restore CA1822 // Mark members as static
}