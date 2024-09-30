using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Gami.Desktop.Models;

public class LibraryUpdateState(LibraryUpdateStateKind kind, float progress = 0) : ReactiveObject
{
    [Reactive] public LibraryUpdateStateKind Kind { get; set; } = kind;

    [Reactive] public float Progress { get; set; } = progress;

    public void Set(LibraryUpdateStateKind kind, float progress = 0)
    {
        Kind = kind;
        Progress = progress;
    }

    public override string ToString()
    {
        return $"{Kind}: {Progress}";
    }
}