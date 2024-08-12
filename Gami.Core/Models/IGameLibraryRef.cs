using System.Diagnostics.CodeAnalysis;

namespace Gami.Core.Models;

[SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
public interface IGameLibraryRef
{
    public string Name { get; set; }
    public string LibraryType { get; set; }
    public string LibraryId { get; set; }
}