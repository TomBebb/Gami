using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Gami.Base;
using Gami.Db.Models;
using Serilog;
using ValveKeyValue;

namespace Gami.Steam;

public sealed class SteamScanner : IGameLibraryScanner
{
    private static string AppsPath => OperatingSystem.IsWindows()
        ? @"C:\Program Files (x86)\Steam\steamapps"
        : Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Steam/steamapps");

    private static GameLibraryRef MapGameManifest(string path)
    {
        Log.Debug("MapGameMan {Path}", path);
        var stream = File.OpenRead(path);
        Log.Debug("MapGame Opened stream {Path}", path);
        var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
        Log.Debug("MapGame created deserializer {Path}", path);
        KVObject data = kv.Deserialize(stream);
        Log.Debug("MapGame deserialized {Path}", path);
        var appId = data["appid"]?.ToString();
        Log.Debug("Raw appId: {AppId}", appId);
        var name = data["name"].ToString();

        Log.Debug("Raw name: {AppId}", name);
        var bytesToDl = data["BytesToDownload"].ToString();
        Log.Debug("Raw BytesToDownload: {AppId}", bytesToDl);

        var bytesDl = data["BytesDownloaded"].ToString();
        Log.Debug("Raw BytesDownloaded: {AppId}", bytesDl);

        var mapped = new GameLibraryRef()
        {
            LibraryType = SteamCommon.TypeName,
            LibraryId = appId,
            Name = name,
            InstallStatus = bytesDl == bytesToDl
                ? GameInstallStatus.Installed
                : GameInstallStatus.Installing
        };

        Log.Debug("Mapped bytes: {Mapped}", JsonSerializer.Serialize(mapped));
        return mapped;
    }


    public async IAsyncEnumerable<IGameLibraryRef> Scan()
    {
        var path = AppsPath;
        if (!Path.Exists(path))
        {
            await Console.Error.WriteLineAsync("Non-existent scan path: " + path);
            yield break;
        }

        Log.Debug("Scanning steam games in {Dir}", path);
        foreach (var partialPath in Directory.GetFiles(path, @"appmanifest*"))
        {
            var manifestPath = Path.Combine(path, partialPath);
            Log.Debug("Mapping game manifest at {Path}", manifestPath);
            var mapped = MapGameManifest(manifestPath);
            Log.Debug("Mapped game manifest at {Path}", manifestPath);
            var name = mapped.Name;
            if (name == "Steam Controller Configs" || name.StartsWith("Steam Linux") || name.StartsWith("Proton") ||
                name.StartsWith("Steamworks"))
                continue;
            yield return mapped;
        }
    }
}