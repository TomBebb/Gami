using System;
using System.Threading.Tasks;
using Gami.Base;
using Instances;

namespace Gami.Steam;

public sealed class SteamCommon : IGameLibraryLauncher, IGameLibraryInstaller
{
    // TODO: Support flatpak, read from registry on windows
    private readonly string _steamPath =
        OperatingSystem.IsWindows() ? "C:/Program Files (x86)/Steam/steam.exe" : "/usr/bin/steam";

    public async ValueTask Install(string id)
    {
        await Instance.FinishAsync(_steamPath, $"steam://install/{id}");
    }

    public string Type => "Steam";

    public async ValueTask Launch(string id)
    {
        await Instance.FinishAsync(_steamPath, $"steam://launch/{id}");
    }
}