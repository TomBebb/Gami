using System.Collections.Frozen;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gami.Base;
using Gami.Db.Models;
using Gami.Steam;

namespace Gami.Db;

public static class GameExtensions
{
    private static readonly FrozenDictionary<string, IGameLibraryInstaller> InstallersByName =
        new KeyValuePair<string, IGameLibraryInstaller>[]
        {
            new("steam", SteamCommon.Instance)
        }.ToFrozenDictionary();

    private static readonly FrozenDictionary<string, IGameLibraryLauncher> LaunchersByName =
        new KeyValuePair<string, IGameLibraryLauncher>[]
        {
            new("steam", SteamCommon.Instance)
        }.ToFrozenDictionary();

    public static void Launch(this Game game)
    {
        LaunchersByName[game.LibraryType].Launch(game.LibraryId);
    }

    public static void Install(this Game game)
    {
        InstallersByName[game.LibraryType].Install(game.LibraryId);
    }
}