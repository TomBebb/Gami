using System.Collections.Generic;
using Gami.Db.Models;

namespace Gami.Base;

public interface IGameLibraryScanner
{
    public IAsyncEnumerable<IGameLibraryRef> Scan();
}