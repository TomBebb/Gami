using System;
using System.Diagnostics;
using Gami.Desktop.Base;

namespace Gami.Desktop.Steam;

public sealed class SteamCommon : IGameLibraryLauncher, IGameLibraryInstaller
{
    public static readonly SteamCommon Instance = new();

    // TODO: Support flatpak, read from registry on windows
    private static readonly string _steamPath =
        OperatingSystem.IsWindows() ? "C:/Program Files (x86)/Steam/steam.exe" : "steam";

    private static void RunGameCmd(string cmd, string id)
    {
        var info = new ProcessStartInfo { FileName = _steamPath, Arguments = $"steam://{cmd}/{id}" };
        new Process() { StartInfo = info }.Start();
    }

    public void Install(string id) => RunGameCmd("install", id);
    public string Type => "steam";
    public const string TypeName= "steam";

    public void Launch(string id) => RunGameCmd("rungameid", id);
}