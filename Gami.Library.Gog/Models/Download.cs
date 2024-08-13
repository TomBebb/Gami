using System.Diagnostics.CodeAnalysis;

namespace Gami.Library.Gog.Models;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class Download
{
    public string ManualUrl { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Version { get; set; } = null!;
    public string Date { get; set; } = null!;
    public string Size { get; set; } = null!;
}