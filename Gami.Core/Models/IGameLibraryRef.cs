namespace Gami.Core.Models;

public interface IGameLibraryRef
{
    public string Name { get; set; }
    public string LibraryType { get; set; }
    public string LibraryId { get; set; }
}