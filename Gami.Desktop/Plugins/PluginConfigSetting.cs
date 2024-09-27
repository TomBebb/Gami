using System.Diagnostics.CodeAnalysis;
using ReactiveUI.Fody.Helpers;

namespace Gami.Desktop.Plugins;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class PluginConfigSetting
{
    [Reactive] public required string Key { get; set; }
    [Reactive] public required string Name { get; set; }
}