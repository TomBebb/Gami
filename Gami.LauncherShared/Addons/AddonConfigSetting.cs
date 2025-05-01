using System.Diagnostics.CodeAnalysis;
using ReactiveUI.Fody.Helpers;

namespace Gami.LauncherShared.Addons;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class AddonConfigSetting
{
    [Reactive] public required string Key { get; init; }
    [Reactive] public required string Name { get; set; }

    [Reactive] public string? Hint { get; set; }

    [Reactive] public AddonConfigSettingType Type { get; set; } = AddonConfigSettingType.String;
}