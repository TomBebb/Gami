using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Gami.Core;
using Gami.Core.Models;

namespace Gami.Scanner.Steam;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class SteamCommon : IGameLibraryLauncher, IGameLibraryManagement
{
    public const string TypeName = "steam";

    // TODO: Support flatpak, read from registry on windows
    private static readonly string SteamPath =
        OperatingSystem.IsWindows() ? "C:/Program Files (x86)/Steam/steam.exe" : "steam";

    public async ValueTask Install(string id) =>
        await Task.Run(() => RunGameCmd("install", id));

    public void Uninstall(string id) =>
        RunGameCmd("uninstall", id);

    public ValueTask<GameInstallStatus> CheckInstallStatus(string id) =>
        SteamScanner.CheckStatus(id);


    public string Type => "steam";

    public void Launch(string id) =>
        RunGameCmd("rungameid", id);

    public async ValueTask<Process?> GetMatchingProcess(IGameLibraryRef gameRef)
    {
        var meta = SteamScanner.ScanInstalledGame(gameRef.LibraryId);
        if (meta == null)
            return null;
        var appDir = Path.Join(SteamScanner.AppsPath, "common", meta.InstallDir);
        return appDir.ResolveMatchingProcess();
    }


    private static void RunGameCmd(string cmd, string id)
    {
        var info = new ProcessStartInfo
            { FileName = SteamPath, Arguments = $"steam://{cmd}/{id}" };
        new Process { StartInfo = info }.Start();
    }
}