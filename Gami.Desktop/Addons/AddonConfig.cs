using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using Gami.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Gami.Desktop.Addons;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class AddonConfig : ReactiveObject
{
    [Reactive] public required string Key { get; set; }
    [Reactive] public required string Name { get; set; }

    [Reactive] public string? Hint { get; set; }

    [Reactive]
    public required ImmutableArray<AddoConfigSetting> Settings { get; init; } =
        ImmutableArray<AddoConfigSetting>.Empty;


    [JsonIgnore]
    private ImmutableDictionary<string, object> MySettings
    {
        get => AddonJson.Load<ImmutableDictionary<string, object>>(Key) ?? ImmutableDictionary<string, object>.Empty;
        set => AddonJson.Save(value, Key).AsTask().Wait();
    }

    [JsonIgnore]
    public ImmutableArray<MappedAddoConfigSetting> MappedSettings
    {
        get
        {
            var vals = MySettings;
            return
            [
                ..Settings.Select(s => new MappedAddoConfigSetting
                {
                    Key = s.Key,
                    Name = s.Name,
                    Value = vals.GetValueOrDefault(s.Key) ?? "",
                    Hint = s.Hint,
                    Type = s.Type
                })
            ];
        }
        set { MySettings = value.ToImmutableDictionary(v => v.Key, v => v.Value)!; }
    }
}