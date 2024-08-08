using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Text.Json;
using Flurl;
using Gami.Core;
using Gami.Core.Models;
using Serilog;

namespace Gami.Scanner.Steam;

public sealed class SteamAchievementsScanner : IGameAchievementScanner
{
    private sealed class PlayerAchievementItem
    {
        public string ApiName { get; set; }
        public byte Achieved { get; set; }
        public long UnlockTime { get; set; }
    }

    private sealed class PlayerAchievements
    {
        public ImmutableArray<PlayerAchievementItem> Achievements { get; set; }
    }

    private sealed class PlayerAchievementsResults
    {
        public PlayerAchievements PlayerStats { get; set; }
    }

    private sealed class GameSchemaGameStats
    {
        public ImmutableArray<GameSchemaAchievement> Achievements { get; set; }
    }

    private sealed class GameSchema
    {
        public GameSchemaGameStats AvailableGameStats { get; set; } = null!;
    }

    private sealed class GameSchemaResult
    {
        public GameSchema Game { get; set; } = null!;
    }

    private sealed class GameSchemaAchievement
    {
        public int Hidden { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Icon { get; set; }
        public string IconGray { get; set; }
    }

    private SteamConfig _config =
        PluginJson.Load<SteamConfig>(SteamCommon.TypeName) ??
        throw new ApplicationException(
            "steam.json must be manually created for now");

    public string Type => SteamCommon.TypeName;

    private async ValueTask<PlayerAchievementsResults> GetPlayerAchievements
        (IGameLibraryRef game)
    {
        var url =
            "https://api.steampowered.com/ISteamUserStats/GetPlayerAchievements/v0001/"
                .AppendQueryParam("appid", game.LibraryId)
                .AppendQueryParam("key", _config.ApiKey)
                .AppendQueryParam("steamid", _config.SteamId);

        Log.Debug("Fetch playerachievements for {GameId}", url);

        var res = await HttpConsts.HttpClient.GetFromJsonAsync<PlayerAchievementsResults>(
            url,
            new
                JsonSerializerOptions(JsonSerializerDefaults.Web));
        return res!;
    }

    private async ValueTask<GameSchemaResult> GetGameAchievements
        (IGameLibraryRef game)
    {
        var url =
            "https://api.steampowered.com/ISteamUserStats/GetSchemaForGame/v2/"
                .AppendQueryParam("appid", game.LibraryId)
                .AppendQueryParam("key", _config.ApiKey);

        Log.Debug("Fetch game aachievements for {GameId}", url);


        var res = await HttpConsts.HttpClient.GetFromJsonAsync<GameSchemaResult>(url,
            new
                JsonSerializerOptions(JsonSerializerDefaults.Web));

        return res!;
    }

    public async ValueTask<ConcurrentBag<Achievement>> Scan(IGameLibraryRef game)
    {
        var allAchievements = await GetGameAchievements(game).ConfigureAwait(false);
        var res = new ConcurrentBag<Achievement>();
        if (allAchievements.Game?.AvailableGameStats?.Achievements == null)
            return res;
        Log.Debug("Game achievements: {Game}",
            allAchievements.Game.AvailableGameStats.Achievements.Length);

        var client = HttpConsts.HttpClient;
        await Task.WhenAll(allAchievements.Game.AvailableGameStats.Achievements.Select(
            async
                achievement =>
            {
                Log.Debug("Fetching icons for {Name}", achievement.DisplayName);
                var icons = await Task.WhenAll(new[]
                {
                    client.GetByteArrayAsync(achievement.Icon),
                    client.GetByteArrayAsync(achievement.IconGray)
                });
                Log.Debug("Fetched icons for {Name}", achievement.DisplayName);

                res.Add(new Achievement()
                {
                    Id =
                        $"{game.LibraryType}:{game.LibraryId}::{achievement.Name}",
                    Name = achievement.DisplayName,
                    LibraryId = achievement.Name,
                    LockedIcon = icons[0],
                    UnlockedIcon = icons[1]
                });
            }));
        return res;
    }

    public async ValueTask<ConcurrentBag<AchievementProgress>> ScanProgress(
        IGameLibraryRef game)
    {
        var res = new ConcurrentBag<AchievementProgress>();
        var playerAchievements =
            await GetPlayerAchievements(game).ConfigureAwait(false);
        Log.Debug("Player Achievements: {Achievements}",
            playerAchievements.PlayerStats.Achievements.Length);
        foreach (var achievement in playerAchievements.PlayerStats.Achievements)
            res.Add(new AchievementProgress()
            {
                AchievementId =
                    $"{game.LibraryType}:{game.LibraryId}::{achievement.ApiName}",
                UnlockTime = DateTime.UnixEpoch.AddSeconds(achievement.UnlockTime),
                Unlocked = achievement.Achieved == 1
            });
        return res;
    }
}