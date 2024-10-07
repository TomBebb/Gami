using Gami.Core.Models;
using ReactiveUI.Fody.Helpers;

namespace Gami.Desktop.ViewModels;

public sealed class EditGameViewModel: ViewModelBase
{
    [Reactive]
    public Game EditingGame { get; set; }
}