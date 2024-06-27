using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Gami.Base;
using Gami.Db.Schema.Metadata;

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
    public static readonly string AppListCachePath = Path.Join(App.AppDir, "gami");

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

        foreach (var file in paths)
        {
            var appId = Path.GetFileName(file);
            yield return new GameLibraryRef
            {
                LibraryId = appId,
                LibraryType = "steam",
                Name = appListById[int.Parse(appId)].Name
            };
        }
    }

    public static async ValueTask<AppListResult?> GetAppList()
    {
        var diff = File.Exists(AppListCachePath)
            ? DateTime.UtcNow - File.GetLastWriteTimeUtc(AppListCachePath)
            : TimeSpan.Zero;
        if (!Path.Exists(AppListCachePath) ||
            DateTime.UtcNow - File.GetLastWriteTimeUtc(AppListCachePath) > TimeSpan.FromMinutes(30))
        {
            Console.WriteLine($"Fetch AppList:{AppListCachePath}; diff: {diff}");
            var res = await GetAppListRaw();
            Console.WriteLine($"Fetched AppList:{res?.AppList?.Apps?.Count ?? 0}");
            var resText = JsonSerializer.Serialize(res);
            await File.WriteAllTextAsync(AppListCachePath, resText);
            return res;
        }

        Console.WriteLine("Read AppList:" + AppListCachePath);
        var text = await File.ReadAllTextAsync(AppListCachePath);
        return JsonSerializer.Deserialize<AppListResult>(text, SerializerSettings.JsonOptions);
    }

    public static Task<AppListResult?> GetAppListRaw()
    {
        var client = new HttpClient();
        return client.GetFromJsonAsync<AppListResult>("https://api.steampowered.com/ISteamApps/GetAppList/v2",
            SerializerSettings.JsonOptions);
    }
}