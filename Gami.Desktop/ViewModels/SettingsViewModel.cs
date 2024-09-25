using System.IO;
using System.Reactive;
using Gami.Core;
using Gami.Desktop.Models;
using ReactiveUI;

namespace Gami.Desktop.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    public static readonly string ConfigPath = Path.Join(Consts.AppDir, "settings.json");

    public SettingsViewModel()
    {
        SaveCommand = ReactiveCommand.Create(() => Settings.Save());
    }

    public MySettings Settings { get; set; } = MySettings.Load();

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
}