using Avalonia.Controls;
using Gami.BigPicture.Views;
using ReactiveUI.Fody.Helpers;

namespace Gami.BigPicture.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        CurrView = new LibraryView { DataContext = CurrViewModel };
    }

    [Reactive] public ViewModelBase? CurrViewModel { get; set; } = new LibraryViewModel();
    [Reactive] public UserControl? CurrView { get; set; }
}