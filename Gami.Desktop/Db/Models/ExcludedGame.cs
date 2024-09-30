using System.ComponentModel.DataAnnotations;

namespace Gami.Desktop.Db.Models;

public sealed class ExcludedGame
{
    [MaxLength(40)] public string LibraryType { get; init; } = string.Empty;

    [MaxLength(500)] public string LibraryId { get; init; } = string.Empty;
}