using System.Diagnostics.CodeAnalysis;

namespace Gami.Core.Models;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public sealed class Developer : NamedIdItem
{
    public List<Game> Games { get; set; } = null!;
}