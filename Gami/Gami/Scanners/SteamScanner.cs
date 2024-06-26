using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Gami.Db.Schema.Metadata;

namespace Gami.Scanners;

public sealed class AppListItem
{
    [JsonPropertyName("appid")] public string AppId { get; set; } = null!;

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
    private static string ScanPath => OperatingSystem.IsWindows()
        ? @"C:\Program Files (x86)\Steam\steamapps\shadercache"
        : Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".steam/steamapps/compatdata");

    public IEnumerable<IGameLibraryRef> Scan()
    {
        var path = ScanPath;
        if (!Path.Exists(path)) yield break;
        foreach (var file in Directory.EnumerateFiles(path))
            yield return new GameLibraryRef
            {
                LibraryId = Path.GetFileName(file),
                LibraryType = "steam"
            };
    }

    public static Task<AppListResult?> GetAppListRaw()
    {
        var client = new HttpClient();
        return client.GetFromJsonAsync<AppListResult>("https://api.steampowered.com/ISteamApps/GetAppList/v2/",
            SerializerSettings.JsonOptions);
    }
}