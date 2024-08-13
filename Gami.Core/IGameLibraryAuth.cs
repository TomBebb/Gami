namespace Gami.Core;

public interface IGameLibraryAuth : IBasePlugin
{
    public bool NeedsAuth { get; }

    /// <summary>
    /// Process a URL change
    /// </summary>
    /// <param name="url">The URL the auth webview changed to</param>
    /// <returns>True when the URL means we can authenticate no</returns>
    public ValueTask<bool> CurrUrlChange(string url);

    /// <summary>
    /// Get the authentication URL
    /// </summary>
    /// <returns></returns>
    public string AuthUrl();
}