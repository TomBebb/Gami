using Gami.Core.Models;
using ReactiveUI.Fody.Helpers;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Gami.Desktop.ViewModels;

public sealed class EditGameViewModel : ViewModelBase
{
    [Reactive] public Game EditingGame { get; set; } = null!;
}