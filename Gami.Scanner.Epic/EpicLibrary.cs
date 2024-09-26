using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Gami.Core;
using Gami.Core.Models;
using Serilog;

namespace Gami.Scanner.Epic;

public sealed partial class EpicLibrary : IGameLibraryManagement, IGameLibraryLauncher, IGameLibraryScanner
{
    private static readonly Regex OutputReg = MyRegex();

    private static readonly JsonSerializerOptions LegendaryJsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private static ImmutableDictionary<string, InstallationData> ScanInstalledData()
    {
        var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config/legendary/installed.json");
        if (!File.Exists(path))
            return ImmutableDictionary<string, InstallationData>.Empty;

        var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<ImmutableDictionary<string, InstallationData>>(stream, LegendaryJsonSerializerOptions) ?? ImmutableDictionary<string, InstallationData>.Empty;
    }

    public string Type => "epic";

    public async IAsyncEnumerable<IGameLibraryMetadata> Scan()
    {
        var installData = ScanInstalledData();

        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "legendary",
                Arguments = "list",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        proc.Start();

        var text = await proc.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
        Log.Debug("Got epic games text");
        var matches = OutputReg.Matches(text);
        foreach (Match match in matches)
        {
            var id = match.Groups[2].Value;
            Log.Debug("Scanned match groups {Groups}", match.Groups);
            yield return new ScannedGameLibraryMetadata
            {
                LibraryType = Type,
                Name = match.Groups[1].Value,
                LibraryId = id,
                InstallStatus = installData.ContainsKey(id) ? GameInstallStatus.Installed : GameInstallStatus.InLibrary
            };
        }
    }

    public void Launch(IGameLibraryRef gameRef)
    {
        var id = gameRef.LibraryId;
        var datas = ScanInstalledData();
        var fullExe = Path.Join(datas[id].InstallPath, datas[id].Executable);

        Process.Start(fullExe);
    }

    public ValueTask<Process?> GetMatchingProcess(IGameLibraryRef gameRef) =>
        ValueTask.FromResult(ScanInstalledData()[gameRef.LibraryId].InstallPath.ResolveMatchingProcess());

    public async ValueTask Install(IGameLibraryRef gameRef)
    {
        var id = gameRef.LibraryId;
        await Task.Run(() =>
            Process.Start("legendary", ["install", id, "-y"]).Start()
        );
    }

    public void Uninstall(IGameLibraryRef gameRef)
    {
        Process.Start("legendary", ["uninstall", gameRef.LibraryId, "-y"]).Start();
    }

    public ValueTask<GameInstallStatus> CheckInstallStatus(IGameLibraryRef game) => ValueTask.FromResult(ScanInstalledData()
        .ContainsKey(game.LibraryId)
        ? GameInstallStatus.Installed
        : GameInstallStatus.InLibrary);

    [GeneratedRegex(@" \* (.+) \(App name: (.+) \| Version: (.+)\)")]
    private static partial Regex MyRegex();
}