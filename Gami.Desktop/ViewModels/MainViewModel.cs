using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text.Json;
using EFCore.BulkExtensions;
using Gami.Core.Models;
using Gami.Desktop.Db;
using Gami.Desktop.Models;
using Gami.Desktop.Plugins;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace Gami.Desktop.ViewModels;

public class MainViewModel : ViewModelBase
{
    [Reactive] public MappedGame SelectedGame { get; set; }

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
        RefreshCache();
    }

    public void RefreshCache()
    {
        using (var db = new GamiContext())
        {
            Games = new ObservableCollection<MappedGame>(db.Games.Select(v => new MappedGame()
            {
                LibraryType = v.LibraryType,
                LibraryId = v.LibraryId,
                InstallStatus = v.InstallStatus,
                Name = v.Name,
                GameGenres = v.GameGenres.Select(g => new GameGenre
                {
                    Genre = new Genre { Name = g.Genre.Name }
                }).ToList(),
                Description = v.Description,
                Icon = v.Icon
            }));
        }

        Games.CollectionChanged += (sender, args) =>
        {
            using var db = new GamiContext();
            db.BulkUpdateAsync(Games);
        };
    }
#pragma warning disable CA1822 // Mark members as static

    [Reactive] public MappedGame? EditingGame { get; set; }

    public ReactiveCommand<Game, Unit> PlayGame { get; }
    public ReactiveCommand<MappedGame, Unit> EditGame { get; set; }
    public ReactiveCommand<Game, Unit> InstallGame { get; set; }
    public ReactiveCommand<Game, Unit> UninstallGame { get; set; }
    public ReactiveCommand<Unit, Unit> Refresh { get; set; }


    public ObservableCollection<MappedGame> Games { get; private set; } = new();

#pragma warning restore CA1822 // Mark members as static
}