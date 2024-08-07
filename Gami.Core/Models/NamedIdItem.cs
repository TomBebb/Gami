using System.ComponentModel.DataAnnotations;

namespace Gami.Core.Models;

public class NamedIdItem
{
    [Key] public int Id { get; set; }

    public string Name { get; set; } = null!;
}