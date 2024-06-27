using System;
using System.Threading.Tasks;
using Cysharp.Diagnostics;
using Gami.Base;

namespace Gami.Steam;

public sealed class SteamCommon : IGameLibraryLauncher, IGameLibraryInstaller
{
    // TODO: Support flatpak, read from registry on windows
    private readonly string _steamPath =
        OperatingSystem.IsWindows() ? "C:/Program Files (x86)/Steam/steam.exe" : "/usr/bin/steam";

    public async ValueTask Install(string id)
    {
        await ProcessX.StartAsync($"{_steamPath} steam://install/{id}").WaitAsync();
    }

    public string Type => "Steam";

    public async ValueTask Launch(string id)
    {
        await ProcessX.StartAsync($"{_steamPath} steam://launch/{id}").WriteLineAllAsync();
    }
}