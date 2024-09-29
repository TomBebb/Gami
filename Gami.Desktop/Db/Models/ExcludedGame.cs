using System.ComponentModel.DataAnnotations;

namespace Gami.Desktop.Db.Models;

public sealed class ExcludedGame
{
    [MaxLength(40)] public string LibraryType { get; set; } = string.Empty;

    [MaxLength(500)] public string LibraryId { get; set; } = string.Empty;
}