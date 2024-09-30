using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Gami.Desktop.ViewModels;

namespace Gami.Desktop.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void NavigationView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        (DataContext as MainViewModel)!.Curr = e.SelectedItem.ToString()!;
    }
}