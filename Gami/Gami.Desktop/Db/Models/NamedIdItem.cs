using System.ComponentModel.DataAnnotations;

namespace Gami.Desktop.Db.Models;

public class NamedIdItem
{
    [Key] public int Id { get; set; }

    public string Name { get; set; } = null!;
}