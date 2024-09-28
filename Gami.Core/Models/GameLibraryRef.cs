namespace Gami.Core.Models;

public class GameLibraryRef : IGameLibraryRef
{
    public string Name { get; set; } = null!;
    public string LibraryType { get; set; } = null!;
    public string LibraryId { get; set; } = null!;
}