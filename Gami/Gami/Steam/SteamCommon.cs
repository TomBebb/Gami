using System;
using System.Threading.Tasks;
using Gami.Base;

namespace Gami.Steam;

public sealed class SteamCommon : IGameLibraryLauncher, IGameLibraryInstaller
{
    public static readonly SteamCommon Instance = new();

    // TODO: Support flatpak, read from registry on windows
    private readonly string _steamPath =
        OperatingSystem.IsWindows() ? "C:/Program Files (x86)/Steam/steam.exe" : "steam";

    public async ValueTask Install(string id)
    {
        await $"{_steamPath} steam://install/{id}".RunShellAsync();
    }

    public string Type => "Steam";

    public async ValueTask Launch(string id)
    {
        Console.WriteLine("Steam launch:"+id);
        await $"{_steamPath} steam://launch/{id}".RunShellAsync();
    }
}