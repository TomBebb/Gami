using System.Diagnostics.CodeAnalysis;

namespace Gami.Desktop.Plugins;

public enum PluginSettingType
{
    String,
    Int,
    Bool
}

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed class PluginValueConfig
{
    public required string Key { get; set; }
    public required string Name { get; set; }

    public PluginSettingType Type { get; set; } = PluginSettingType.String;
}

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PluginConfig
{
    public required string Key { get; set; }
    public required string Name { get; set; }
}