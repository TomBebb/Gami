using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using Gami.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Gami.Desktop.Plugins;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class PluginConfig : ReactiveObject
{
    [Reactive] public required string Key { get; set; }
    [Reactive] public required string Name { get; set; }

    [Reactive]
    public required ImmutableArray<PluginConfigSetting> Settings { get; init; } =
        ImmutableArray<PluginConfigSetting>.Empty;


    [JsonIgnore]
    private FrozenDictionary<string, object> MySettings
    {
        get => PluginJson.Load<FrozenDictionary<string, object>>(Key)!;
        set => PluginJson.Save(value, Key).AsTask().Wait();
    }

    [JsonIgnore]
    public ImmutableArray<MappedPluginConfigSetting> MappedSettings
    {
        get
        {
            var vals = MySettings;
            return
            [
                ..Settings.Select(s => new MappedPluginConfigSetting
                {
                    Key = s.Key,
                    Name = s.Name,
                    Value = vals[s.Key],
                    Hint = s.Hint
                })
            ];
        }
        set { MySettings = value.ToFrozenDictionary(v => v.Key, v => v.Value)!; }
    }
}