using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DynamicData.Binding;
using Gami.Core.Models;
using Gami.Desktop.Db;
using Gami.Desktop.MIsc;
using Gami.Desktop.Models;
using Gami.Desktop.Plugins;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace Gami.Desktop.ViewModels;

public class MainViewModel : ViewModelBase
{
    private static readonly TimeSpan LookupProcessInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan LookupProcessTimeout = TimeSpan.FromMinutes(2);
    [Reactive] public string Search { get; set; }
    [Reactive] public MappedGame? SelectedGame { get; set; } = null!;
    [Reactive] public bool IsPlayingSelected { get; set; }

    [Reactive] public Game? PlayingGame { get; set; }

    [Reactive] public Process? Current { get; set; }

    public ImmutableArray<Wrapped<string>> SortFields { get; set; } = Enumerable.Range(0, (int)SortGameField.Max)
        .Cast<SortGameField>()
        .Select(v => new Wrapped<string>(v
            .GetName())).ToImmutableArray();

    [Reactive] public int SortFieldIndex { get; set; }

    public MainViewModel()
    {
        PlayGame = ReactiveCommand.CreateFromTask(async (Game game) =>
        {
            Log.Information("Play game: {Game}", JsonSerializer.Serialize(game));
            game.Launch();

            PlayingGame = game;

            var start = DateTime.UtcNow;
            while (Current == null && DateTime.UtcNow - start < LookupProcessTimeout)
            {
                await Task.Delay(LookupProcessInterval);
                Current = await GameExtensions.LaunchersByName[game.LibraryType].GetMatchingProcess(game);
            }

            Log.Debug("Game open: {Open}", Current != null);
            if (Current != null)
            {
                Current.EnableRaisingEvents = true;
                Current.Exited += (_, _) =>
                {
                    PlayingGame = null;
                    Log.Debug("Game closed");
                };
            }
            else
            {
                PlayingGame = null;
            }
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
        ExitGame = ReactiveCommand.Create(() => { Current?.Kill(true); });

        this.WhenAnyValue(v => v.Search, v => v.SortFieldIndex).ForEachAsync((_) => RefreshCache());
        RefreshCache();

        this.WhenAnyValue(v => v.PlayingGame, v => v.SelectedGame)
            .Select(v => v.Item1?.LibraryId == v.Item2?.LibraryId && v.Item1?.LibraryType == v.Item2?.LibraryType)
            .BindTo(this, x => x.IsPlayingSelected);
    }


    public void RefreshCache()
    {
        var sort = (SortGameField)SortFieldIndex;
        Log.Debug("Refresh cache - Search: {Search}; Sort: {Sort}", Search, sort);
        var dir = SortDirection.Ascending;
        using var db = new GamiContext();
        var games = db.Games
            .Where(v => string.IsNullOrEmpty(Search) || EF.Functions.Like(v.Name, $"%{Search}%"));

        switch (sort)
        {
            case SortGameField.Name:
                games = games.Sort(v => v.Name, dir);
                break;
            case SortGameField.LibraryType:
                games = games.Sort(v => v.LibraryType, dir);
                break;
            case SortGameField.ReleaseDate:
                games = games.Sort(v => v.ReleaseDate, dir);
                break;
            case SortGameField.InstallStatus:
                games = games.Sort(v => v.InstallStatus, dir);
                break;
        }

        Games = games.Select(v => new MappedGame()
        {
            LibraryType = v.LibraryType,
            LibraryId = v.LibraryId,
            InstallStatus = v.InstallStatus,
            Name = v.Name,
            Description = v.Description,
            Icon = v.Icon,
            Playtime = v.Playtime,
            ReleaseDate = v.ReleaseDate
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
    public ReactiveCommand<Unit, Unit> ExitGame { get; set; }


    [Reactive] public ImmutableList<MappedGame> Games { get; private set; } = ImmutableList<MappedGame>.Empty;

#pragma warning restore CA1822 // Mark members as static
}