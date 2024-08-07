using System.Collections.Generic;

namespace Gami.Desktop.Db.Models;

public sealed class Feature : NamedIdItem
{
    public List<GameFeature> GameFeatures { get; set; }
}