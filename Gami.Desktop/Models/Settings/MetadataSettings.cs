using System.Collections.Immutable;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Gami.Desktop.Models.Settings;

public sealed class MetadataSettings : ReactiveObject
{
    [Reactive] public bool FetchAchievements { get; set; } = true;
    [Reactive] public bool FetchMetadata { get; set; } = true;

    [Reactive]
    public ImmutableSortedSet<string> Sources { get; set; } =
        ImmutableSortedSet<string>.Empty.Add("Store");
}