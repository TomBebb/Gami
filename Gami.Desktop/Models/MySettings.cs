using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    [Reactive] public bool ShowSystemTrayIcon { get; set; } = true;
    [Reactive] public bool MinimizeToSystemTray { get; set; }
    [Reactive] public bool MinimizeToSystemTrayOnClose { get; set; }

    [Reactive]
    public ImmutableSortedSet<string> MetadataNameSources { get; set; } =
        ImmutableSortedSet<string>.Empty.Add("Matching");

    [JsonIgnore]
    public ImmutableArray<WrappedText> MappedMetadataNameSources
    {
        get => MetadataNameSources.Select(v => new WrappedText(v)).ToImmutableArray();
        set => MetadataNameSources = value.Select(v => v.Data).ToImmutableSortedSet();
    }

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