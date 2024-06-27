﻿using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Gami.Base;
using Gami.Db.Schema.Metadata;
using Instances;

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

        var paths = OperatingSystem.IsWindows() ? Directory.EnumerateFiles(path) : Directory.EnumerateDirectories(path);
        var appListRes = await GetAppList();
        var appListById = appListRes?.AppList.Apps.ToFrozenDictionary(k => k.AppId) ??
                          FrozenDictionary<int, AppListItem>.Empty;
        if (appListById.Count == 0)

            throw new ApplicationException("Invalid app list");
        Console.WriteLine("AppList: " + string.Join(", ", appListById.Keys.Take(5)) + " paths: "+string.Join(", ", paths) );
        foreach (var file in paths)
        {
            var appId = Path.GetFileName(file);
            yield return new GameLibraryRef
            {
                LibraryId = appId,
                LibraryType = "steam",
                Name = appListById.GetValueOrDefault(int.Parse(appId))?.Name ?? "???"
            };
        }
    }

    public static async ValueTask<AppListResult?> GetAppList()
    {
        if (!Directory.Exists(App.AppDir))
            Directory.CreateDirectory(App.AppDir);
        var diff = File.Exists(AppListCachePath)
            ? DateTime.UtcNow - File.GetLastWriteTimeUtc(AppListCachePath)
            : TimeSpan.Zero;
        if (!Path.Exists(AppListCachePath) ||
            DateTime.UtcNow - File.GetLastWriteTimeUtc(AppListCachePath) > TimeSpan.FromMinutes(30))
        {
            await Instance.FinishAsync("curl",
                $"https://api.steampowered.com/ISteamApps/GetAppList/v2 -o '{AppListCachePath}'");

        }

        var text = File.ReadAllText(AppListCachePath);
        return JsonSerializer.Deserialize<AppListResult>(text, SerializerSettings.JsonOptions);
    }
}