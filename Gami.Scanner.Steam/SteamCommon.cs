using System.Collections.Immutable;
using System.Diagnostics;
using F23.StringSimilarity;
using Gami.Core;
using Gami.Core.Models;
using Serilog;
using GlobExpressions;

namespace Gami.Scanner.Steam;

public sealed class SteamCommon : IGameLibraryLauncher, IGameLibraryManagement
{
    public const string TypeName = "steam";
    public static readonly SteamCommon Instance = new();

    // TODO: Support flatpak, read from registry on windows
    private static readonly string SteamPath =
        OperatingSystem.IsWindows() ? "C:/Program Files (x86)/Steam/steam.exe" : "steam";

    public void Install(string id) =>
        RunGameCmd("install", id);

    public void Uninstall(string id) =>
        RunGameCmd("uninstall", id);

    public string Type => "steam";

    public void Launch(string id) =>
        RunGameCmd("rungameid", id);

    public async ValueTask<Process?> GetMatchingProcess(IGameLibraryRef gameRef)
    {
        var meta = SteamScanner.ScanInstalledGame(gameRef.LibraryId);
        var appDir = Path.Join(SteamScanner.AppsPath, "common", meta.InstallDir);
        var exes = Glob.Files(pattern: "**/*.exe", workingDirectory: appDir).ToImmutableHashSet();

        Log.Debug("Steam CheckOpen: {Curr}; game executables: {Exes}", gameRef, exes);


        return exes.SelectMany(exe => Process.GetProcessesByName(exe.Replace(".exe", "")))
            .FirstOrDefault();
    }


    private static void RunGameCmd(string cmd, string id)
    {
        var info = new ProcessStartInfo
            { FileName = SteamPath, Arguments = $"steam://{cmd}/{id}" };
        new Process { StartInfo = info }.Start();
    }
}