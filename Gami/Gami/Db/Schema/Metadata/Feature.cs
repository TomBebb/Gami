using System.Collections.Generic;

namespace Gami.Db.Schema.Metadata;

public sealed class Feature : NamedIdItem
{
    public List<GameFeature> GameFeatures { get; set; }
}