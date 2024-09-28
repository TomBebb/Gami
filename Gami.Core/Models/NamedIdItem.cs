using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Gami.Core.Models;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class NamedIdItem
{
    [Key] public int Id { get; set; }

    public string Name { get; set; } = null!;
}