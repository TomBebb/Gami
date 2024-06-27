using System.Collections.Generic;
using Gami.Db.Schema.Metadata;

namespace Gami.Base;

public interface IGameLibraryScanner
{
    public IAsyncEnumerable<IGameLibraryRef> Scan();
}