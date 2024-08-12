using System.Collections.Concurrent;
using Gami.Core.Models;

namespace Gami.Core;

public interface IGameAchievementScanner : IBasePlugin
{
    public ValueTask<ConcurrentBag<Achievement>> Scan(IGameLibraryRef game);

    public ValueTask<ConcurrentBag<AchievementProgress>> ScanProgress(
        IGameLibraryRef game);
}