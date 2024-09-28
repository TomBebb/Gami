﻿using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using FluentAvalonia.UI.Data;
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

        this.WhenAnyValue(v => v.Filter).Subscribe(_ => { ReloadAchievements(); });
        this.WhenAnyValue(v => v.Sort).Subscribe(_ => { ReloadAchievements(); });
        this.WhenAnyValue(v => v.SortDirection).Subscribe(_ => { ReloadAchievements(); });
        ToggleSortDir = ReactiveCommand.Create(() =>
        {
            SortDirection = SortDirection == SortDirection.Ascending
                ? SortDirection.Descending
                : SortDirection.Ascending;
        });
        this.WhenAnyValue(v => v.SortDirection).Subscribe(dir => { SortAscending = dir == SortDirection.Ascending; });
    }

    [Reactive] public ImmutableArray<Game> Games { get; set; }

    [Reactive] public AchievementsFilter Filter { get; set; } = AchievementsFilter.None;
    [Reactive] public AchievementSort Sort { get; set; } = AchievementSort.UnlockTime;

    [Reactive] public SortDirection SortDirection { get; set; } = SortDirection.Descending;
    [Reactive] public bool SortAscending { get; set; }
    public string[] FilterOptions => Enum.GetValues<AchievementsFilter>().Select(af => af.Humanize()).ToArray();
    public string[] SortOptions => Enum.GetValues<AchievementSort>().Select(af => af.Humanize()).ToArray();

    public ReactiveCommand<Unit, Unit> ToggleSortDir { get; }

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

        var achievementsQuery = db.Achievements.AsQueryable().Where(a => a.GameId == SelectedGame.Id)
            .Where(a => Filter == AchievementsFilter.None ||
                        Filter == AchievementsFilter.Locked == !a.Progress.Unlocked);


        achievementsQuery = (Sort, SortDirection) switch
        {
            (AchievementSort.Name, SortDirection.Ascending) => achievementsQuery.OrderBy(a => a.Name),
            (AchievementSort.Name, SortDirection.Descending) => achievementsQuery.OrderByDescending(a =>
                a.Name),
            (AchievementSort.GlobalProgress, SortDirection.Ascending) => achievementsQuery.OrderBy(a =>
                a.GlobalPercent),
            (AchievementSort.GlobalProgress, SortDirection.Descending) => achievementsQuery.OrderByDescending(a =>
                a.GlobalPercent),

            (AchievementSort.UnlockTime, SortDirection.Ascending) => achievementsQuery.OrderBy(a =>
                a.Progress == null ? DateTime.UnixEpoch : a.Progress.UnlockTime),
            (AchievementSort.UnlockTime, SortDirection.Descending) => achievementsQuery.OrderByDescending(a =>
                a.Progress == null ? DateTime.UnixEpoch : a.Progress.UnlockTime),
            (AchievementSort.Unlocked, SortDirection.Ascending) => achievementsQuery.OrderBy(a =>
                a.Progress != null && a.Progress.Unlocked),
            (AchievementSort.Unlocked, SortDirection.Descending) => achievementsQuery.OrderByDescending(a =>
                a.Progress != null && a.Progress.Unlocked)
        };

        Achievements = achievementsQuery
            .Select(a => new AchievementData(a, a.Progress ?? new AchievementProgress()))
            .ToImmutableArray();

        Log.Debug("Selected game changed! Fetched achievements");
    }
}