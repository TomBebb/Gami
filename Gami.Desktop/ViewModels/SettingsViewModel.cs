using System;
using System.Reactive;
using Gami.Desktop.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace Gami.Desktop.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    public SettingsViewModel()
    {
        SaveCommand = ReactiveCommand.Create(() =>
        {
            Settings.Save();
            SettingsChanged?.Invoke(Settings);
        });
    }

    [Reactive] public MySettings Settings { get; set; } = MySettings.Load();

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public static event Action<MySettings> SettingsChanged;

    public void Watch()
    {
        Log.Information("Watching settings...");
        SettingsChanged += v => Settings = v;
    }
}