namespace Gami.Core.Models;

public sealed class Feature : NamedIdItem
{
    public List<GameFeature> GameFeatures { get; set; }
}