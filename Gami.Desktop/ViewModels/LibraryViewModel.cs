using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using Gami.Core;
using Gami.Core.Models;
using Gami.Desktop.Db;
using Gami.Desktop.Db.Models;
using Gami.Desktop.MIsc;
using Gami.Desktop.Models;
using Gami.Desktop.Models.Settings;
using Gami.Desktop.Plugins;
using Gami.Library.Gog;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using WebView = AvaloniaWebView.WebView;

namespace Gami.Desktop.ViewModels;

public class LibraryViewModel : ViewModelBase
{
    private static readonly TimeSpan LookupProcessInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan LookupProcessTimeout = TimeSpan.FromMinutes(2);


    public LibraryViewModel()
    {
        DeleteGame = ReactiveCommand.CreateFromTask(async (Game game) =>
        {
            async ValueTask DeleteGame()
            {
                await using var db = new GamiContext();
                await db.Games.Where(g => g.Id == game.Id).ExecuteDeleteAsync();
                RefreshCache();
            }

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
        });
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
            }
        });
        InstallGame = ReactiveCommand.CreateFromTask(async (Game game) =>
        {
            Log.Information("Install game: {Game}", JsonSerializer.Serialize(game));
            await game.Install();
            var status = await GameExtensions.InstallersByName[game.LibraryType].CheckInstallStatus(game);
            await using var db = new GamiContext();
            game.InstallStatus = status;
            game.Id = $"{game.LibraryType}:{game.LibraryId}";

            db.Games.Attach(game);
            db.Entry(game).Property(x => x.InstallStatus).IsModified = true;
            await db.SaveChangesAsync();
            RefreshCache();
        });
        UninstallGame = ReactiveCommand.CreateFromTask(async (Game game) =>
        {
            Log.Information("Uninstall game: {Game}", JsonSerializer.Serialize(game));
            game.Uninstall();
            var status = await GameExtensions.InstallersByName[game.LibraryType].CheckInstallStatus(game);
            await using var db = new GamiContext();
            game.InstallStatus = status;
            game.Id = $"{game.LibraryType}:{game.LibraryId}";
            db.Games.Attach(game);
            db.Entry(game).Property(x => x.InstallStatus).IsModified = true;
            await db.SaveChangesAsync();
            RefreshCache();
        });
        EditGame = ReactiveCommand.Create((Game game) =>
        {
            Log.Information("Edit game: {Game}", JsonSerializer.Serialize(game));
            EditingGame = game;
        });
        ShowDialog = ReactiveCommand.CreateFromTask(async (string? initialUrl) =>
        {
            if (initialUrl != null)
                CurrentUrl = initialUrl;
            if (CurrentUrl == null)
                return;
            var webview = new WebView { Url = new Uri(CurrentUrl!), MinHeight = 400, MinWidth = 400 };
            webview.NavigationStarting += (_, arg) => CurrentUrl = arg.Url?.ToString() ?? CurrentUrl;
            var dialog = new ContentDialog
            {
                Title = "My Dialog Title",
                CloseButtonText = "Close",
                Content = webview
            };
            await dialog.ShowAsync();
            this.WhenAnyValue(v => v.CurrentUrl)
                .Subscribe(async v =>
                {
                    Log.Debug("weebbview URL changed: {Url}", v);
                    if (v == null || Auth == null)
                        return;
                    if (await Auth!.CurrUrlChange(v)) dialog?.Hide(ContentDialogResult.Primary);
                });
        });
        ClearSearch = ReactiveCommand.Create(() => { Search = ""; });
        ExitGame = ReactiveCommand.Create(() => { Current?.Kill(true); });
        this.WhenAnyValue(v => v.CurrentUrl)
            .Subscribe(v => Log.Debug("URL changed: {Url}", v));
        this.WhenAnyValue(v => v.Search, v => v.SortFieldIndex)
            .Subscribe(_ => RefreshCache());
        RefreshCache();

        this.WhenAnyValue(v => v.Auth)
            .Select(v => v?.AuthUrl())
            .BindTo(this, v => v.CurrentUrl);
        this.WhenAnyValue(v => v.PlayingGame, v => v.SelectedGame)
            .Select(v => v.Item1?.LibraryId == v.Item2?.LibraryId && v.Item1?.LibraryType == v.Item2?.LibraryType)
            .BindTo(this, x => x.IsPlayingSelected);

        RefreshGame = ReactiveCommand.CreateFromTask((string input) => Refresh(input).AsTask());
    }

    public ImmutableArray<PluginConfig> Plugins =>
    [
        new()
        {
            Key = "all",
            Name = "All",
            Settings = ImmutableArray<PluginConfigSetting>.Empty
        },
        ..GameExtensions.PluginConfigs.Values.Where(v => GameExtensions.ScannersByName.ContainsKey(v.Key))
    ];

    [Reactive] public string Search { get; set; } = "";
    [Reactive] public Game? SelectedGame { get; set; }
    [Reactive] public bool IsPlayingSelected { get; set; }

    [Reactive] private Game? PlayingGame { get; set; }

    [Reactive] private Process? Current { get; set; }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
    [Reactive] private IGameLibraryAuth? Auth { get; set; } = new GogLibrary();
    [Reactive] public string? CurrentUrl { get; set; }
    public ReactiveCommand<string?, Unit> RefreshGame { get; }

    public ImmutableArray<string> SortFields { get; set; } =
    [
        ..Enum.GetValues(typeof(SortGameField))
            .Cast<SortGameField>()
            .Select(v => v
                .GetName())
    ];

    [Reactive] public int SortFieldIndex { get; set; }

    private async ValueTask Refresh(string key)
    {
        var settings = await MySettings.LoadAsync();
        Log.Information("Refresh: {Name}", key);
        if (key == "all")
        {
            await DbOps.ScanAllLibraries();
            if (settings.Metadata.FetchAchievements)
                await DbOps.ScanAchievementsData();
            if (settings.Metadata.FetchMetadata)
                await DbOps.ScanMetadata();
        }
        else
        {
            var scanner = GameExtensions.ScannersByName[key];
            await DbOps.ScanLibrary(scanner);


            if (settings.Metadata.FetchAchievements &&
                GameExtensions.AchievementsByName.TryGetValue(key, out var value))
                await DbOps.ScanAchievementsData(value);

            if (settings.Metadata.FetchMetadata)
                await DbOps.ScanMetadata(key);
        }


        RefreshCache();
    }

    private void RefreshCache()
    {
        var sort = (SortGameField)SortFieldIndex;
        Log.Debug("Refresh cache - Search: {Search}; Sort: {Sort}", Search, sort);
        var dir = SortDirection.Ascending;
        using var db = new GamiContext();
        var games = db.Games
            .Where(v => string.IsNullOrEmpty(Search) || EF.Functions.Like(v.Name, $"%{Search}%"));

        games = sort switch
        {
            SortGameField.Name => games.Sort(v => v.Name, dir),
            SortGameField.LibraryType => games.Sort(v => v.LibraryType, dir),
            SortGameField.ReleaseDate => games.Sort(v => v.ReleaseDate, dir),
            SortGameField.InstallStatus => games.Sort(v => v.InstallStatus, dir),
            _ => games
        };

        Games = games
            .ToImmutableList();
    }
#pragma warning disable CA1822 // Mark members as static

    [Reactive] public Game? EditingGame { get; set; }

    public ReactiveCommand<Unit, Unit> ClearSearch { get; }
    public ReactiveCommand<Game, Unit> PlayGame { get; }
    public ReactiveCommand<Game, Unit> EditGame { get; set; }
    public ReactiveCommand<Game, Unit> InstallGame { get; set; }
    public ReactiveCommand<Game, Unit> UninstallGame { get; set; }
    public ReactiveCommand<Game, Unit> DeleteGame { get; }
    public ReactiveCommand<Unit, Unit> ExitGame { get; set; }
    public ReactiveCommand<string?, Unit> ShowDialog { get; set; }


    [Reactive] public ImmutableList<Game> Games { get; private set; } = ImmutableList<Game>.Empty;

#pragma warning restore CA1822 // Mark members as static
}