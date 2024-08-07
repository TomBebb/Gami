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

    public static ValueTask Launch(this Game game)
    {
        return LaunchersByName[game.LibraryType].Launch(game.LibraryId);
    }

    public static ValueTask Install(this Game game)
    {
        return InstallersByName[game.LibraryType].Install(game.LibraryId);
    }
}