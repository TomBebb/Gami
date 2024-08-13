using System.Collections.Immutable;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using Flurl;
using Gami.Core;
using Gami.Core.Models;
using Gami.Library.Gog.Models;
using Serilog;
using TupleAsJsonArray;

namespace Gami.Library.Gog;

public sealed class GogLibrary : IGameLibraryAuth, IGameLibraryScanner, IGameLibraryManagement
{
    private const string ClientId = "46899977096215655";
    private const string ClientSecret = "9d85c43b1482497dbbce61f6e4aa173a433796eeae2ca8c5f6129f2dc4de46d9";
    private const string RedirectUri = "https://embed.gog.com/on_login_success?origin=client";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new TupleConverterFactory() }
    };

    private static readonly JsonSerializerOptions AuthSerializerOptions = new(JsonSerializerDefaults.General)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public string Type => "gog";
    private static readonly HttpClient HttpClient = new();

    private ValueTask<T> GetAuthJson<T>(string uri) => GetAuthJson<T>(new Uri(uri));

    private async ValueTask<T> GetAuthJson<T>(Uri uri)
    {
        await AutoRefreshToken();
        var request = new HttpRequestMessage()
        {
            RequestUri = uri,
            Method = HttpMethod.Get
        };
        request.Headers.Add("Authorization", $"Bearer {_config.AccessToken}");
        Log.Debug("GOG fetching {Uri}", uri);
        var res = await HttpClient.SendAsync(request).ConfigureAwait(false);
        Log.Debug("GOG fetched {Uri}", uri);
        var stream = await res.Content.ReadAsStreamAsync();
        Log.Debug("GOG fetched as stream {Uri}", uri);

        var demo = new GameDetails()
        {
            Downloads = ImmutableArray<(string, ImmutableDictionary<string, ImmutableArray<Download>>)>.Empty
                .Add(("English", ImmutableDictionary<string, ImmutableArray<Download>>.Empty.Add("windows",
                    ImmutableArray.Create<Download>().Add(new Download { })
                )))
        };
        Log.Debug("Download demo {Demo}", JsonSerializer.Serialize(demo, SerializerOptions));
        var data = await JsonSerializer.DeserializeAsync<T>(stream, SerializerOptions);
        Log.Debug("GOG deserialized {Uri}", uri);
        return data!;
    }

    private ValueTask<OwnedGames> GetOwnedGames() => GetAuthJson<OwnedGames>("https://embed.gog.com/user/data/games");
    private ValueTask<GameDetails> GetGameDetails(string id) => GetAuthJson<GameDetails>("https://embed.gog.com/account/gameDetails/".AppendPathSegment($"{id}.json"));

    public async IAsyncEnumerable<IGameLibraryMetadata> Scan()
    {
        if (string.IsNullOrEmpty(_config.AccessToken))
        {
            Log.Error("Not authenticated; returning none");

            yield break;
        }

        Log.Debug("GetOwnedGams");

        var ownedGames = await GetOwnedGames().ConfigureAwait(false);
        Log.Debug("GotOwnedGams");
        foreach (var gameIdLong in ownedGames.Owned)
        {
            Log.Debug("Scan game ID {Id}", gameIdLong);
            var gameId = gameIdLong.ToString();
            var game = await GetGameDetails(gameId);
            Log.Debug("Scanned game ID {Id}", gameIdLong);

            yield return new ScannedGameLibraryMetadata()
            {
                LibraryType = Type,
                Name = game.Title,
                Playtime = TimeSpan.Zero,
                LibraryId = gameId,
                InstallStatus = _config.InstalledGames.ContainsKey(gameIdLong) ? GameInstallStatus.Installed : GameInstallStatus.InLibrary
            };
        }
    }

    public bool NeedsAuth => true;

    private readonly MyConfig _config = PluginJson.Load<MyConfig>("gog") ??
                                        new MyConfig();

    private ValueTask SaveConfig() => PluginJson.Save(_config, "gog");

    private async ValueTask ProcessTokenUrl(string url)
    {
        Log.Debug("Get gog token");

        var auth = await HttpClient.GetFromJsonAsync<AuthTokenResponse>(url, AuthSerializerOptions).ConfigureAwait(false);
        Log.Debug("Got gog token {Data}", JsonSerializer.Serialize(auth));
        _config.AccessToken = auth!.AccessToken;
        _config.RefreshToken = auth!.RefreshToken;
        _config.AccessTokenExpire = DateTime.UtcNow + TimeSpan.FromSeconds(auth.ExpiresIn);

        Log.Debug("Saving config");
        await SaveConfig();
        Log.Debug("Saved config");
    }

    private async ValueTask ProcessLoginCode(string code)
    {
        var url = "https://auth.gog.com/token"
            .SetQueryParam("client_id", ClientId)
            .SetQueryParam("client_secret", ClientSecret)
            .SetQueryParam("redirect_uri", RedirectUri)
            .SetQueryParam("grant_type", "authorization_code")
            .SetQueryParam("code", code);
        await ProcessTokenUrl(url);
    }

    private async ValueTask RefreshToken()
    {
        var url = "https://auth.gog.com/token"
            .SetQueryParam("client_id", ClientId)
            .SetQueryParam("client_secret", ClientSecret)
            .SetQueryParam("redirect_uri", RedirectUri)
            .SetQueryParam("grant_type", "refresh_token")
            .SetQueryParam("refresh_token", _config.RefreshToken);
        await ProcessTokenUrl(url);
    }

    private async ValueTask AutoRefreshToken()
    {
        Log.Debug("Expire time: {Expire} Now: {Now}", _config.AccessTokenExpire, DateTime.UtcNow);
        if (_config.AccessTokenExpire.HasValue && DateTime.UtcNow > _config.AccessTokenExpire.Value)
            await RefreshToken().ConfigureAwait(false);
    }

    public async ValueTask<bool> CurrUrlChange(string url)
    {
        Log.Debug("URL changed {Code}", url);

        var isAuth = url.StartsWith("https://embed.gog.com/on_login_success");
        if (!isAuth)
            return false;

        var parsed = new Uri(url);

        var ps = HttpUtility.ParseQueryString(parsed.Query);
        var code = ps.Get("code") ?? throw new NullReferenceException("No code param!");
        Log.Debug("Got gog code {Code}", code);
        await ProcessLoginCode(code);
        return true;
    }

    public string AuthUrl() => "https://auth.gog.com/auth?client_id=46899977096215655&redirect_uri=https%3A%2F%2Fembed.gog.com%2Fon_login_success%3Forigin%3Dclient&response_type=code&layout=client2";

    public async ValueTask Install(string id)
    {
        var details = await GetGameDetails(id);
        Log.Debug("Game details {Json}", JsonSerializer.Serialize(details, SerializerOptions));
        var dlDir = Path.Join(Consts.AppDir, "gog/dls");
        Directory.CreateDirectory(dlDir);

        var osMap = details.Downloads.FirstOrDefault(v => v.Item1 == "English").Item2;


        Log.Debug("OS Map {Json}", JsonSerializer.Serialize(osMap, SerializerOptions));
        var dls = osMap["windows"];
        Log.Debug("Mapped URLs {Json}", JsonSerializer.Serialize(dls, SerializerOptions));

        string ResolveDlPath(Download dl) => Path.Join(dlDir, Path.GetFileName(dl.ManualUrl) + ".exe");
        foreach (var currDl in dls)
        {
            Log.Debug("Get url {Url}", currDl.ManualUrl);

            var outPath = ResolveDlPath(currDl);
            Log.Debug("Saving to {Out}", outPath);
            await using var dlStream = await HttpClient.GetStreamAsync("https://embed.gog.com" + currDl.ManualUrl);

            Log.Debug("Opened dl stream {Out}", outPath);
            await using var outStream = File.OpenWrite(outPath);
            await dlStream.CopyToAsync(outStream);
            outStream.Close();

            Log.Debug("Downloaded file to {Path}", outPath);
        }

        Process.Start(ResolveDlPath(dls[0]));
    }

    public void Uninstall(string id)
    {
        throw new NotImplementedException();
    }

    public ValueTask<GameInstallStatus> CheckInstallStatus(string id) => ValueTask.FromResult(_config.InstalledGames
        .ContainsKey(long.Parse(id))
        ? GameInstallStatus.Installed
        : GameInstallStatus.InLibrary);
}