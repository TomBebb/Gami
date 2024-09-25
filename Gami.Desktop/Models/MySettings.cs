using System.IO;
using System.Text.Json;
using Gami.Core;
using Gami.Core.Models;
using Gami.Desktop.Plugins;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace Gami.Desktop.Models;

public class MySettings : Settings
{
    public static readonly string ConfigPath = Path.Join(Consts.AppDir, "settings.json");

    [Reactive] public GameLaunchBehavior GameLaunchWindowBehavior { get; set; } = GameLaunchBehavior.Minimize;


    public static MySettings Load()
    {
        var info = new FileInfo(ConfigPath);
        if (!info.Exists || info.Length == 0) return new MySettings();
        using var stream = File.OpenRead(ConfigPath);
        return JsonSerializer.Deserialize<MySettings>(stream, GameExtensions.PluginOpts)!;
    }

    public void Save()
    {
        using var stream = File.Open(ConfigPath, FileMode.Create);
        JsonSerializer.Serialize(stream, this, GameExtensions.PluginOpts);
        Log.Debug("Saved settings to {Path}", ConfigPath);
    }
}