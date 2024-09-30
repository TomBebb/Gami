using System.Collections.Frozen;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Gami.Core;
using Gami.Core.Models;
using Serilog;

// ReSharper disable UnusedType.Global

namespace Gami.Scanner.Epic;

public sealed partial class EpicLibrary : IGameLibraryManagement, IGameLibraryLauncher, IGameLibraryScanner
{
    private static readonly Regex OutputReg = MyRegex();

    private static readonly JsonSerializerOptions LegendaryJsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public void Launch(IGameLibraryRef gameRef)
    {
        var id = gameRef.LibraryId;
        var datas = ScanInstalledData();
        var fullExe = Path.Join(datas[id].InstallPath, datas[id].Executable);

        Process.Start(fullExe);
    }

    public ValueTask<Process?> GetMatchingProcess(IGameLibraryRef gameRef) =>
        ValueTask.FromResult(ScanInstalledData()[gameRef.LibraryId].InstallPath.ResolveMatchingProcess());

    public string Type => "epic";

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

    public ValueTask<GameInstallStatus> CheckInstallStatus(IGameLibraryRef game) =>
        ValueTask.FromResult(ScanInstalledData()
            .ContainsKey(game.LibraryId)
            ? GameInstallStatus.Installed
            : GameInstallStatus.InLibrary);

    public async IAsyncEnumerable<IGameLibraryMetadata> Scan()
    {
        var text = "N/A";

        FrozenDictionary<string, InstallationData>? installData = null;
        try
        {
            installData = ScanInstalledData();

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

            text = await proc.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Log.Warning("Error while scanning apps: {Err}", ex.Message);
        }


        if (installData == null)
            yield break;
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
                InstallStatus = installData.ContainsKey(id)
                    ? GameInstallStatus.Installed
                    : GameInstallStatus.InLibrary
            };
        }
    }

    private static FrozenDictionary<string, InstallationData> ScanInstalledData()
    {
        var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config/legendary/installed.json");
        if (!File.Exists(path))
            return FrozenDictionary<string, InstallationData>.Empty;

        var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<FrozenDictionary<string, InstallationData>>(stream,
            LegendaryJsonSerializerOptions) ?? FrozenDictionary<string, InstallationData>.Empty;
    }

    [GeneratedRegex(@" \* (.+) \(App name: (.+) \| Version: (.+)\)")]
    private static partial Regex MyRegex();
}