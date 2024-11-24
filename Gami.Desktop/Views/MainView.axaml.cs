using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Gami.Desktop.ViewModels;
using Serilog;

namespace Gami.Desktop.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void NavigationView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        Log.Debug("NavigationView_OnSelectionChanged: {Selection}", e.SelectedItem);
        (DataContext as MainViewModel)!.CurrRoute = (Route)e.SelectedItem ;

    }
}