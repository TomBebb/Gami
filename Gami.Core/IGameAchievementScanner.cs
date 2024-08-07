using System.Collections.Concurrent;
using Gami.Core.Models;

namespace Gami.Core;

public interface IGameAchievementScanner
{
    public string Type { get; }
    public ValueTask<ConcurrentBag<Achievement>> Scan(IGameLibraryRef game);
}