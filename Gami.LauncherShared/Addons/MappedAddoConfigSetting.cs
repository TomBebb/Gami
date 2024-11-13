using System.Diagnostics.CodeAnalysis;
using ReactiveUI.Fody.Helpers;

namespace Gami.LauncherShared.Addons;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class MappedAddoConfigSetting : AddoConfigSetting
{
    [Reactive] public object? Value { get; set; }
}