using System;
using System.Reactive;
using Gami.LauncherShared.Models.Settings;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

// ReSharper disable MemberCanBePrivate.Global

namespace Gami.Desktop.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    public SettingsViewModel()
    {
        SaveCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await Settings.SaveAsync();
            SettingsChanged(Settings);
        });
    }

    [Reactive] public MySettings Settings { get; set; } = MySettings.Load();

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public static event Action<MySettings> SettingsChanged = null!;

    public void Watch()
    {
        Log.Information("Watching settings...");
        SettingsChanged += v => Settings = v;
    }
}