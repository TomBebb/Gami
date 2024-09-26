using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Gami.Desktop.Plugins;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PluginConfig : ReactiveObject
{
    [Reactive] public required string Key { get; set; }
    [Reactive] public required string Name { get; set; }

    [Reactive]
    public required ImmutableArray<PluginConfigSetting> Settings { get; set; } =
        ImmutableArray<PluginConfigSetting>.Empty;
}

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PluginConfigSetting
{
    [Reactive] public required string Key { get; set; }
    [Reactive] public required string Name { get; set; }
}