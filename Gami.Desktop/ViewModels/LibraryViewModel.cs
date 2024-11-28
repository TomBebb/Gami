using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using Gami.Core.Models;
using Gami.Desktop.Misc;
using Gami.Desktop.Views;
using Gami.LauncherShared.Addons;
using Gami.LauncherShared.Db;
using Gami.LauncherShared.Db.Models;
using Gami.LauncherShared.Misc;
using Gami.LauncherShared.Models;
using Gami.LauncherShared.Models.Settings;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

// ReSharper disable MemberCanBeMadeStatic.Global

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

// ReSharper disable MemberCanBePrivate.Global

namespace Gami.Desktop.ViewModels;

public class LibraryViewModel : ViewModelBase
{
    private static readonly TimeSpan CheckInstallInterval = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan LookupProcessInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan LookupProcessTimeout = TimeSpan.FromMinutes(2);


    public LibraryViewModel()
    {
        DeleteGame = ReactiveCommand.CreateFromTask(async (Game game) =>
        {
            var dialog = new ContentDialog
            {
                Title = "Confirm Game Removal",
                CloseButtonText = "No",
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "Yes (add to blacklist)",
                PrimaryButtonCommand = ReactiveCommand.CreateFromTask(async () => await DeleteGame()),
                SecondaryButtonCommand = ReactiveCommand.CreateFromTask(async () =>
                {
                    await DeleteGame();

                    await using var db = new GamiContext();
                    var res = await db.ExcludedGames.AddAsync(new ExcludedGame
                        { LibraryType = game.LibraryType, LibraryId = game.LibraryId });
                    await db.SaveChangesAsync();
                    Log.Debug("Excluded Game: {Game}", res);
                }),
                Content = new Label
                {
                    Content =
                        $"Are you sure you want to remove {game.Name}?\nSelecting \"Yes (add to blacklist)\" will mean the game will not be imported on the next import"
                }
            };
            await dialog.ShowAsync();
            return;

            async ValueTask DeleteGame()
            {
                await using var db = new GamiContext();
                await db.Games.Where(g => g.Id == game.Id).ExecuteDeleteAsync();
                RefreshCache();
            }
        });
        PlayGame = ReactiveCommand.CreateFromTask(async (Game game) =>
        {
            Log.Information("Play game: {Game}", JsonSerializer.Serialize(game));
            game.Launch();

            PlayingGame = game;
            await DbOps.UpdateTimesAsync(game);

            var start = DateTime.UtcNow;
            while (Current == null && DateTime.UtcNow - start < LookupProcessTimeout)
            {
                await Task.Delay(LookupProcessInterval);
                Current = await GamiAddons.LaunchersByName[game.LibraryType].GetMatchingProcess(game);
            }

            Log.Debug("Game open: {Open}", Current != null);
            if (Current != null)
            {
                var actualStart = DateTime.UtcNow;
                Current.EnableRaisingEvents = true;
                Current.Exited += async (_, _) =>
                {
                    await DbOps.UpdateTimesAsync(PlayingGame, DateTime.UtcNow - actualStart);
                    PlayingGame = null;
                    Log.Debug("Game closed");
                };
            }
            else
            {
                PlayingGame = null;
            }

            var mainWindow = WindowUtil.GetMainWindow();
            var settings = MySettings.Load();
            Log.Debug("Main window: {MainWindow}, settings: {Settings}", mainWindow, settings);
            if (mainWindow == null)
                throw new NullReferenceException("Main window is null");
            switch (settings.GameLaunchWindowBehavior)
            {
                case GameLaunchBehavior.DoNothing:
                    break;
                case GameLaunchBehavior.Close:
                    mainWindow.Close();
                    break;

                case GameLaunchBehavior.Minimize:
                    mainWindow.WindowState = WindowState.Minimized;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(settings.GameLaunchWindowBehavior));
            }
        });
        InstallGame = ReactiveCommand.CreateFromTask(async (Game game) =>
        {
            Log.Information("Install game: {Game}", game.Name);
            await game.Install();

            using var _ = game.WhenAnyValue(g => g.InstallStatus).Subscribe(_ =>
            {
                game.SaveInstallState();
                RefreshCache();
            });
            var library = GamiAddons.LibraryManagersByName[game.LibraryType];
            while (game.InstallStatus != GameInstallStatus.Installed)
            {
                await Task.Delay(CheckInstallInterval);
                game.InstallStatus = await library.CheckInstallStatus(game);
            }
        });
        UninstallGame = ReactiveCommand.CreateFromTask(async (Game game) =>
        {
            Log.Information("Uninstall game: {Game}", game.Name);
            game.Uninstall();

            using var _ = game.WhenAnyValue(g => g.InstallStatus).Subscribe(_ =>
            {
                game.SaveInstallState();
                RefreshCache();
            });
            var library = GamiAddons.LibraryManagersByName[game.LibraryType];
            while (game.InstallStatus == GameInstallStatus.Installed)
            {
                await Task.Delay(CheckInstallInterval);
                game.InstallStatus = await library.CheckInstallStatus(game);
            }
        });
        EditGame = ReactiveCommand.Create((Game game) =>
        {
            var context = new EditGameViewModel
            {
                EditingGame = game
            };
            var saveCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                await using var db = new GamiContext();
                db.Games.Update(game);

                await db.SaveChangesAsync();

                RefreshCache();
            });
            var dialog = new ContentDialog
            {
                Title = "Edit game",
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Cancel",
                PrimaryButtonCommand = saveCmd,
                Content = new GameEditor
                {
                    DataContext = context
                }
            };
            dialog.ShowAsync();
        });
        ClearSearch = ReactiveCommand.Create(() => { Search = ""; });
        ExitGame = ReactiveCommand.Create(() => { Current?.Kill(true); });
        this.WhenAnyValue(v => v.Search, v => v.SortFieldIndex)
            .Subscribe(_ => RefreshCache());
        RefreshCache();

        this.WhenAnyValue(v => v.PlayingGame, v => v.SelectedGame)
            .Select(v => v.Item1?.LibraryId == v.Item2?.LibraryId && v.Item1?.LibraryType == v.Item2?.LibraryType)
            .BindTo(this, x => x.IsPlayingSelected);

        RefreshGame = ReactiveCommand.CreateFromTask((string input) => Refresh(input).AsTask())!;
    }

#pragma warning disable CA1822
    public ImmutableArray<AddonConfig> Plugins =>
#pragma warning restore CA1822
    [
        new()
        {
            Key = "all",
            Name = "All",
            Settings = ImmutableArray<AddoConfigSetting>.Empty
        },
        .. GamiAddons.AddonConfigs.Values.Where(v => GamiAddons.ScannersByName.ContainsKey(v.Key))
    ];

    [Reactive] public string Search { get; set; } = "";
    [Reactive] public Game? SelectedGame { get; set; }
    [Reactive] public bool IsPlayingSelected { get; set; }

    [Reactive] private Game? PlayingGame { get; set; }

    [Reactive] private Process? Current { get; set; }
    public ReactiveCommand<string?, Unit> RefreshGame { get; }

    public ImmutableArray<string> SortFields { get; set; } =
    [
        .. Enum.GetValues(typeof(SortGameField))
            .Cast<SortGameField>()
            .Select(v => v
                .GetName())
    ];

    [Reactive] public int SortFieldIndex { get; set; }

    private async ValueTask Refresh(string key)
    {
        var settings = await MySettings.LoadAsync();
        Log.Information("Refresh: {Name}", key);
        var dialog = new TaskDialog
        {
            Title = "Syncing Library",

            Content = "Scanning Games",
            ShowProgressBar = true,
            XamlRoot = WindowUtil.GetMainWindow()
        };

        if (key != "all" && GamiAddons.AddonConfigs.TryGetValue(key, out var setting))
            dialog.Title = $"{dialog.Title} for {setting.Name}";

        dialog.Opened += async (_, _) =>
        {
            if (key == "all")
            {
                await Task.WhenAll(
                    GamiAddons.ScannersByName.Values.Select(async v => { await DbOps.ScanLibrary(v); }));

                if (settings.Achievements.FetchAchievements)
                {
                    dialog.Content = "Scanning achievements";
                    await Task.Run(async () => await DbOps.ScanAchievementsData(OnProgress));
                }

                if (settings.Metadata.FetchMetadata)
                {
                    dialog.Content = "Scanning metadata";
                    await Task.Run(async () => await DbOps.ScanMetadata(OnProgress));
                }
            }
            else
            {
                dialog.SetProgressBarState(0f, TaskDialogProgressState.Indeterminate);
                await DbOps.ScanLibrary(key);

                if (settings.Achievements.FetchAchievements &&
                    GamiAddons.AchievementsByName.TryGetValue(key, out var value))
                {
                    dialog.Content = "Scanning achievements";
                    await Task.Run(async () => await DbOps.ScanAchievementsData(value, OnProgress));
                }

                if (settings.Metadata.FetchMetadata)
                {
                    dialog.Content = "Scanning metadata";
                    await Task.Run(async () => await DbOps.ScanMetadata(key, OnProgress));
                }
            }

            Dispatcher.UIThread.Post(() => { dialog.Hide(TaskDialogStandardResult.OK); });
        };

        await dialog.ShowAsync();
        RefreshCache();
        return;

        void OnProgress(float progress)
        {
            dialog.SetProgressBarState(progress * 100f, TaskDialogProgressState.Normal);
        }
    }

    private void RefreshCache()
    {
        var sort = (SortGameField)SortFieldIndex;
        Log.Debug("Refresh cache - Search: {Search}; Sort: {Sort}", Search, sort);
        const SortDirection dir = SortDirection.Ascending;
        using var db = new GamiContext();
        var games = db.Games
            .Where(v => string.IsNullOrEmpty(Search) || EF.Functions.Like(v.Name, $"%{Search}%"));

        games = sort switch
        {
            SortGameField.Name => games.Sort(v => v.Name, dir),
            SortGameField.LibraryType => games.Sort(v => v.LibraryType, dir),
            SortGameField.ReleaseDate => games.Sort(v => v.ReleaseDate, dir),
            SortGameField.InstallStatus => games.Sort(v => v.InstallStatus, dir),
            SortGameField.LastPlayed => games.Sort(v => v.LastPlayed, SortDirection.Descending),
            SortGameField.PlayTime => games.Sort(v => v.Playtime, SortDirection.Descending),
            _ => games
        };

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
#pragma warning disable CA1822 // Mark members as static

    public ReactiveCommand<Unit, Unit> ClearSearch { get; }
    public ReactiveCommand<Game, Unit> PlayGame { get; }
    public ReactiveCommand<Game, Unit> EditGame { get; set; }
    public ReactiveCommand<Game, Unit> InstallGame { get; set; }
    public ReactiveCommand<Game, Unit> UninstallGame { get; set; }
    public ReactiveCommand<Game, Unit> DeleteGame { get; }
    public ReactiveCommand<Unit, Unit> ExitGame { get; set; }


    [Reactive] public ImmutableList<Game> Games { get; private set; } = ImmutableList<Game>.Empty;

#pragma warning restore CA1822 // Mark members as static
}