using Gami.Core.Models;

namespace Gami.Core;

public interface IGameAchievementScanner
{
    public string Type { get; }
    public IAsyncEnumerable<Achievement> Scan(IGameLibraryRef game);
}