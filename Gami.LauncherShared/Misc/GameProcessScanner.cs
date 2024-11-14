using System.Diagnostics;
using Gami.Core.Models;
using Gami.LauncherShared.Addons;
using Serilog;

namespace Gami.LauncherShared.Misc;

public static class GameProcessScanner
{
    private static readonly TimeSpan LookupProcessInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan LookupProcessTimeout = TimeSpan.FromSeconds(20);

    public static async ValueTask<Process?> AutoScanProcess(this Game game)
    {
        var start = DateTime.UtcNow;
        while (DateTime.UtcNow - start < LookupProcessTimeout)
        {
            await Task.Delay(LookupProcessInterval);
            var process = await GamiAddons.LaunchersByName[game.LibraryType].GetMatchingProcess(game);
            if (process != null)
                return process;
        }

        Log.Warning("Could not find process for game {Game}", game.Name);
        return null;
    }
}