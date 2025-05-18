using Gami.Core.Models;

namespace Gami.Core;

public interface IGameAchievementScanner : IBaseAddon
{
    public IAsyncEnumerable<Achievement> Scan(IGameLibraryRef game);

    public IAsyncEnumerable<AchievementProgress> ScanProgress(
        IGameLibraryRef game);
}