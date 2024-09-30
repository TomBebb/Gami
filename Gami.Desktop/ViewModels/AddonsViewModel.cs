using System.Collections.Immutable;
using System.Linq;
using Gami.Desktop.Plugins;
using ReactiveUI.Fody.Helpers;
using Serilog;

// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Gami.Desktop.ViewModels;

public sealed class AddonsViewModel : ViewModelBase
{
    public AddonsViewModel()
    {
        Log.Information("Installed addons: {Addons}", Installed.Select(addon => addon.Name));

        SelectedAddon = Installed.First();
    }

#pragma warning disable CA1822
    public ImmutableArray<PluginConfig> Installed => GameExtensions.PluginConfigs.Values;
#pragma warning restore CA1822

    [Reactive] public PluginConfig SelectedAddon { get; set; }
}