﻿using System.Diagnostics.CodeAnalysis;

namespace Gami.Desktop.Plugins;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PluginConfig
{
    public required string Key { get; set; }
    public required string Name { get; set; }
}