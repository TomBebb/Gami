using System.ComponentModel.DataAnnotations;

namespace Gami.Db.Schema;

public class NamedIdItem
{
    [Key] public int Id { get; set; }

    public string Name { get; set; } = null!;
}