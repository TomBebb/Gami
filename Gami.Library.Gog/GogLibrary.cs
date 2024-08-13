using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using Flurl;
using Gami.Core;
using Gami.Core.Models;
using Gami.Library.Gog.Models;
using Serilog;

namespace Gami.Library.Gog;

public sealed class GogLibrary : IGameLibraryAuth, IGameLibraryScanner
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    public string Type => "gog";
    private static readonly HttpClient HttpClient = new();

    private static Task<GameDetails?> GetGameDetails(string id) => HttpClient.GetFromJsonAsync<GameDetails>(
        "https://embed.gog.com/account/gameDetails/".AppendPathSegment($"{id}.json"));

    public IAsyncEnumerable<IGameLibraryMetadata> Scan() => throw new NotImplementedException();

    public bool NeedsAuth => true;

    public bool CurrUrlChange(string url)
    {
        Log.Debug("URL changed {Code}", url);

        var isAuth = url.StartsWith("https://embed.gog.com/on_login_success");
        if (!isAuth)
            return false;

        var parsed = new Uri(url);

        var ps = HttpUtility.ParseQueryString(parsed.Query);
        var code = ps.Get("code") ?? throw new NullReferenceException("No code param!");
        Log.Debug("Got gog code {Code}", code);
        return true;
    }

    public string AuthUrl() => "https://auth.gog.com/auth?client_id=46899977096215655&redirect_uri=https%3A%2F%2Fembed.gog.com%2Fon_login_success%3Forigin%3Dclient&response_type=code&layout=client2";
}