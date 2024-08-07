using System.Collections.Generic;
using Gami.Desktop.Db.Models;

namespace Gami.Desktop.Base;

public interface IGameLibraryScanner
{
    public IAsyncEnumerable<IGameLibraryRef> Scan();
}