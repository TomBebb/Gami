using System;
using System.IO;
using System.Reactive;
using Gami.Desktop.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace Gami.Desktop.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private FileSystemWatcher _watcher;

    public SettingsViewModel()
    {
        SaveCommand = ReactiveCommand.Create(() => Settings.Save());
    }

    [Reactive] public MySettings Settings { get; set; } = MySettings.Load();

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public event Action SettingsChanged;

    public void Watch()
    {
        Log.Information("Watching settings...");
        _watcher = new FileSystemWatcher(Path.GetDirectoryName(MySettings.ConfigPath)!,
            Path.GetFileName(MySettings.ConfigPath));
        _watcher.Changed += (_, ev) =>
        {
            Settings = MySettings.Load();
            SettingsChanged?.Invoke();
        };
        _watcher.EnableRaisingEvents = true;
    }
}