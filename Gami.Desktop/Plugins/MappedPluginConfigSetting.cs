using System.Diagnostics.CodeAnalysis;
using ReactiveUI.Fody.Helpers;

namespace Gami.Desktop.Plugins;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class MappedPluginConfigSetting : PluginConfigSetting
{
    [Reactive] public object? Value { get; set; }
}