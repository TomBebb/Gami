using System.Diagnostics.CodeAnalysis;

namespace Gami.Core.Models;

[SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
public interface IGameImages
{
    public Uri? IconUrl { get; set; }
    public Uri? HeaderUrl { get; set; }
    public Uri? HeroUrl { get; set; }
    public Uri? LogoUrl { get; set; }
    
}