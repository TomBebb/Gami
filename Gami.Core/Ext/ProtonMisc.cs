using System.Net.Http.Headers;
using Gami.Core.Models;
using Octokit;

namespace Gami.Core.Ext;

public static class ProtonMisc
{
    private static readonly HttpClient Client = new();
    private static readonly GitHubClient GitHub = new(new Octokit.ProductHeaderValue("Gami Games launcher"));

    public static string ResolveProtonPrefix(this IGameLibraryRef gameRef) => Path.Join(Consts.ProtonPrefixDir, gameRef.Name);

    public static async ValueTask Download(Release release)
    {
        var matching = release.Assets.FirstOrDefault(a => a.Name.EndsWith(".tar.gz"))!;
        var tarballStream = await Client.GetStreamAsync(matching!.Url);

        await tarballStream.ExtractTarGz(Path.Join(Consts.ProtonDlDir, release.Name));
    }

    public static async ValueTask AutoDownloadLatest(this Settings settings)
    {
        var latest = await GitHub.Repository.Release.GetLatest("GloriousEggroll", "proton-ge-custom");
        await AutoDownload(latest);
        settings.DefaultProton = latest.Name;
    }

    public static ValueTask AutoDownload(Release release)
    {
        if (File.Exists(Path.Join(Consts.ProtonDlDir, release.Name, "proton")))
            return ValueTask.CompletedTask;
        return Download(release);
    }
}