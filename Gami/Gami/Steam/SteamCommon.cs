using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Gami.Base;
using Serilog;

namespace Gami.Steam;

public sealed class SteamCommon : IGameLibraryLauncher, IGameLibraryInstaller
{
    public static readonly SteamCommon Instance = new();

    // TODO: Support flatpak, read from registry on windows
    private readonly string _steamPath =
        OperatingSystem.IsWindows() ? "C:/Program Files (x86)/Steam/steam.exe" : "steam";

    public async ValueTask Install(string id)
    {
        await new ProcessStartInfo { FileName = _steamPath, Arguments = $"steam://install/{id}" }.RunAsync();
    }

    public string Type => "Steam";

    public async ValueTask Launch(string id)
    {
        Log.Debug("Steam launch: {}", id);
        await new ProcessStartInfo { FileName = _steamPath, Arguments = $"steam://launch/{id}" }.RunAsync();
    }
}