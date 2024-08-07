using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Gami.Core;
using Gami.Core.Models;
using Gami.Desktop.Steam;

namespace Gami.Desktop.Db;

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

    private static T GetLauncher<T>(this FrozenDictionary<string, T> dictionary, string libType)
    {
        if (!dictionary.TryGetValue(libType, out var launcher))
            throw new ApplicationException(
                $"No launcher found with library type '{libType}\", valid keys: {string.Join(", ", dictionary.Keys)}");
        return launcher;
    }

    public static void Launch(this Game game)
    {
        LaunchersByName.GetLauncher(game.LibraryType).Launch(game.LibraryId);
    }

    public static void Install(this Game game)
    {
        InstallersByName.GetLauncher(game.LibraryType).Install(game.LibraryId);
    }
}