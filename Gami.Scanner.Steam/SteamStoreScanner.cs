using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using Gami.Core;
using Gami.Core.Models;
using Serilog;

namespace Gami.Scanner.Steam;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed class SteamStoreScanner : IGameMetadataScanner
{
    public string Type => SteamCommon.TypeName;

    private static GameMetadata MapMetadata(AppDetailsData data) =>
        new()
        {
            Description = data.DetailedDescription,
            Developers = data.Developers,
            Publishers = data.Publishers,
            Genres = (data.Genres ?? ImmutableArray<AppGenre>.Empty).Select(g => g.Description).ToImmutableArray()
        };

    public async ValueTask<GameMetadata> ScanMetadata(IGameLibraryRef game)
    {
        var res = await HttpConsts.HttpClient.GetFromJsonAsync<ImmutableDictionary<string, AppDetails>>(
            $"https://store.steampowered.com/api/appdetails?appids={game.LibraryId}", SteamSerializerOptions.JsonOptions).ConfigureAwait(false);

        Log.Debug("Game raw metadata: {Data}", res);
        var data = res?.Values.FirstOrDefault();
        return data == null ? new GameMetadata() : MapMetadata(data.Data);
    }
}