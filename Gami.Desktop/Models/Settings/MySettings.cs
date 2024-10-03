using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Gami.Core;
using Gami.Desktop.Addons;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace Gami.Desktop.Models.Settings;

public class MySettings : Core.Models.Settings
{
    public static readonly string ConfigPath = Path.Join(Consts.AppDir, "settings.json");

    [Reactive] public GameLaunchBehavior GameLaunchWindowBehavior { get; set; } = GameLaunchBehavior.Minimize;

    [Reactive] public bool ShowSystemTrayIcon { get; set; } = true;
    [Reactive] public bool MinimizeToSystemTray { get; set; }
    [Reactive] public bool MinimizeToSystemTrayOnClose { get; set; }

    [Reactive] public MetadataSettings Metadata { get; init; } = new();


    public static MySettings Load()
    {
        var info = new FileInfo(ConfigPath);
        if (!info.Exists || info.Length == 0) return new MySettings();
        using var stream = File.OpenRead(ConfigPath);
        return JsonSerializer.Deserialize<MySettings>(stream, GamiAddons.PluginOpts)!;
    }

    public static async ValueTask<MySettings> LoadAsync()
    {
        var info = new FileInfo(ConfigPath);
        if (!info.Exists || info.Length == 0) return new MySettings();
        await using var stream = File.OpenRead(ConfigPath);
        return (await JsonSerializer.DeserializeAsync<MySettings>(stream, GamiAddons.PluginOpts))!;
    }

    public async ValueTask SaveAsync()
    {
        await using var stream = File.Open(ConfigPath, FileMode.Create);
        await JsonSerializer.SerializeAsync(stream, this, GamiAddons.PluginOpts);
        Log.Debug("Saved settings to {Path}", ConfigPath);
    }
}