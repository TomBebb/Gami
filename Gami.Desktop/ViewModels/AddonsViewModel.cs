using System.Collections.Immutable;
using System.Linq;
using Gami.Desktop.Plugins;
using Serilog;

namespace Gami.Desktop.ViewModels;

public sealed class AddonsViewModel : ViewModelBase
{
    public AddonsViewModel()
    {
        Log.Information("Installed addons: {Addons}", Installed.Select(addon => addon.Name));
    }

    public ImmutableArray<PluginConfig> Installed => GameExtensions.PluginConfigs.Values;
}