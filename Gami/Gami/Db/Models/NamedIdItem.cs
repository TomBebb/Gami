using System.ComponentModel.DataAnnotations;

namespace Gami.Db.Models;

public class NamedIdItem
{
    [Key] public int Id { get; set; }

    public string Name { get; set; } = null!;
}