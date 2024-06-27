using System.Collections.Generic;
using Gami.Db.Schema.Metadata;

namespace Gami.Scanners;

public interface IGameLibraryScanner
{
    public IEnumerable<IGameLibraryRef> Scan();
}