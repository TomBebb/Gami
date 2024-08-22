using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using Flurl;
using Gami.Core;
using Gami.Core.Models;
using Nito.AsyncEx;
using Serilog;

namespace Gami.Scanner.Steam;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Local")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
public sealed class SteamAchievementsScanner : IGameAchievementScanner
{
    private sealed class PlayerAchievementItem
    {
        public required string ApiName { get; set; }
        public byte Achieved { get; set; }
        public long UnlockTime { get; set; }
    }

    private sealed class PlayerAchievements
    {
        public ImmutableArray<PlayerAchievementItem>? Achievements { get; set; }
    }

    private sealed class PlayerAchievementsResults
    {
        public required PlayerAchievements PlayerStats { get; set; }
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
        // ReSharper disable once UnusedMember.Local
        public int Hidden { get; set; }
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public required string Icon { get; set; }
        public required string IconGray { get; set; }
    }

    private readonly  AsyncLazy<SteamConfig> _config = new AsyncLazy<SteamConfig>(async () =>
        await PluginJson.LoadOrErrorAsync< SteamConfig > (SteamCommon.TypeName));

    public string Type => SteamCommon.TypeName;

    private async ValueTask<PlayerAchievementsResults> GetPlayerAchievements
        (IGameLibraryRef game)
    {
        var config = await _config.Task;
        var url =
            "https://api.steampowered.com/ISteamUserStats/GetPlayerAchievements/v0001/"
                .AppendQueryParam("appid", game.LibraryId)
                .AppendQueryParam("key", config.ApiKey)
                .AppendQueryParam("steamid", config.SteamId);

        Log.Debug("Fetch playerachievements for {GameId}", url);
        try
        {
            var res = await HttpConsts.HttpClient
                .GetFromJsonAsync<PlayerAchievementsResults>(
                    url,
                    new
                        JsonSerializerOptions(JsonSerializerDefaults.Web));
            return res!;
        }
        catch (HttpRequestException)
        {
            return new PlayerAchievementsResults
            {
                PlayerStats = new PlayerAchievements { Achievements = ImmutableArray<PlayerAchievementItem>.Empty }
            };
        }
    }

    private async ValueTask<GameSchemaResult> GetGameAchievements
        (IGameLibraryRef game)
    {
        var config = await _config.Task;
        var url =
            "https://api.steampowered.com/ISteamUserStats/GetSchemaForGame/v2/"
                .AppendQueryParam("appid", game.LibraryId)
                .AppendQueryParam("key", config.ApiKey);

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
        if (allAchievements.Game.AvailableGameStats.Achievements == null)
            return res;
        Log.Debug("Game achievements: {Game}",
            allAchievements.Game.AvailableGameStats.Achievements.Length);

        var client = HttpConsts.HttpClient;
        await Task.WhenAll(allAchievements.Game.AvailableGameStats.Achievements.Select(
            async
                achievement =>
            {
                Log.Debug("Fetching icons for {Name}", achievement.DisplayName);
                var icons = await Task.WhenAll([
                    client.GetByteArrayAsync(achievement.Icon),
                    client.GetByteArrayAsync(achievement.IconGray)
                ]);
                Log.Debug("Fetched icons for {Name}", achievement.DisplayName);

                res.Add(new Achievement
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
            playerAchievements.PlayerStats.Achievements?.Length ?? 0);
        foreach (var achievement in playerAchievements.PlayerStats.Achievements ??
                                    ImmutableArray<PlayerAchievementItem>.Empty)
            res.Add(new AchievementProgress
            {
                AchievementId =
                    $"{game.LibraryType}:{game.LibraryId}::{achievement.ApiName}",
                UnlockTime = achievement.UnlockTime == 0
                    ? null
                    : DateTime.UnixEpoch.AddSeconds
                        (achievement.UnlockTime),
                Unlocked = achievement.Achieved == 1
            });
        return res;
    }
}