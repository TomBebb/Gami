﻿using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Gami.Base;
using Gami.Db.Models;
using Instances;
using Serilog;

namespace Gami.Scanners;

public sealed class AppListItem
{
    [JsonPropertyName("appid")] public int AppId { get; set; }

    public string Name { get; set; } = null!;
}

public sealed class AppList
{
    public ImmutableList<AppListItem> Apps { get; set; } = null!;
}

public sealed class AppListResult
{
    [JsonPropertyName("applist")] public AppList AppList { get; set; }
}

public sealed class SteamScanner : IGameLibraryScanner
{
    public static readonly string AppListCachePath = Path.Join(App.AppDir, "steam_cache.json");

    private static string ScanPath => OperatingSystem.IsWindows()
        ? @"C:\Program Files (x86)\Steam\steamapps\shadercache"
        : Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Steam/steamapps/compatdata");

    public async IAsyncEnumerable<IGameLibraryRef> Scan()
    {
        var path = ScanPath;
        if (!Path.Exists(path))
        {
            await Console.Error.WriteLineAsync("Non-existent scan path: " + path);
            yield break;
        }

        var paths = OperatingSystem.IsWindows()
            ? Directory.EnumerateFiles(path)
            : Directory.EnumerateDirectories(path).Except(new[] { "0" });
        var appListRes = await GetAppList();
        var appListById = appListRes?.AppList.Apps.DistinctBy(k => k.AppId).ToFrozenDictionary(k => k.AppId) ??
                          FrozenDictionary<int, AppListItem>.Empty;
        if (appListById.Count == 0)

            throw new ApplicationException("Invalid app list");
        foreach (var file in paths)
        {
            var appId = Path.GetFileName(file);
            var name = appListById.GetValueOrDefault(int.Parse(appId))?.Name;
            if (name == null)
                continue;
            yield return new GameLibraryRef
            {
                LibraryId = appId,
                LibraryType = "steam",
                Name = name
            };
        }
    }

    public static async ValueTask<AppListResult?> GetAppList()
    {
        var hasCacheFile = Path.Exists(AppListCachePath);
        var sinceLastDownload =
            hasCacheFile ? DateTime.UtcNow - File.GetLastWriteTimeUtc(AppListCachePath) : TimeSpan.Zero;
        Log.Debug("Get App List; hasCache={HasCache}, sinceLastDl={SinceLastDownload}", hasCacheFile,
            sinceLastDownload);
        if (!hasCacheFile || sinceLastDownload > TimeSpan.FromMinutes(30))
        {
            Log.Debug("Fetching app list");
            await new ProcessStartInfo("curl",
                $"https://api.steampowered.com/ISteamApps/GetAppList/v2 -o {AppListCachePath}").RunAsync();
            Log.Debug("Fetched app list");
        }

        Log.Debug("Reading from cache: {CachePath}", AppListCachePath);

        var text = File.ReadAllText(AppListCachePath);
        Log.Debug("Got App List");
        return JsonSerializer.Deserialize<AppListResult>(text, SerializerSettings.JsonOptions);
    }
}