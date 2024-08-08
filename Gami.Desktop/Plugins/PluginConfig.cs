namespace Gami.Desktop.Plugins;

public enum PluginSettingType
{
    String,
    Int,
    Bool
}

public sealed class PluginValueConfig
{
    public required string Key { get; set; }
    public required string Name { get; set; }

    public PluginSettingType Type { get; set; } = PluginSettingType.String;
}

public sealed class PluginConfig
{
    public required string Key { get; set; }
    public required string Name { get; set; }
}