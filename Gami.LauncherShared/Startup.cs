using Gami.Core;
using Gami.LauncherShared.Db;
using Gami.LauncherShared.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Gami.LauncherShared;

public static class Startup
{
    public static void CommonSetup()
    {
        Directory.CreateDirectory(Consts.BasePluginDir);
        using (DbContext context = new GamiContext())
        {
            Log.Information("Ensure DB created");
            context.Database.EnsureCreated();
        }


        Log.Information("Save changes");


        Task.Run(async () =>
        {
            var settings = await MySettings.LoadAsync();
            if (settings.Achievements.ScanAchievementsProgressOnStart)
                await DbOps.ScanAchievementsProgress(settings.Achievements);
        });
        Log.Information("Saved changes");
    }
}