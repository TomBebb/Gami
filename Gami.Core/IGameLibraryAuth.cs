namespace Gami.Core;

public interface IGameLibraryAuth : IBasePlugin
{
    public bool NeedsAuth { get; }
    public bool CurrUrlChange(string url);

    public string AuthUrl();
}