using Gami.Core.Models;
using Gami.LauncherShared.Db;

namespace Gami.LauncherShared.Misc;

public static class GameExt
{
    public static void SaveInstallState(this Game game)
    {
        using var db = new GamiContext();
        db.Games.Attach(game);
        db.Entry(game).Property(x => x.InstallStatus).IsModified = true;
        db.SaveChanges();
    }
}