using Avalonia.Controls;
using Avalonia.Interactivity;
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
        (DataContext as MainViewModel)!.CurrRoute = (Route)e.SelectedItem;
    }

    private void Minimize(object? sender, RoutedEventArgs e)
    {
        var window = (Window)Parent;
        window!.WindowState = WindowState.Minimized;
    }

    private void Maximize(object? sender, RoutedEventArgs e)
    {
        var window = (Window)Parent;
        window!.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void Close(object? sender, RoutedEventArgs e)
    {
        var window = (Window)Parent;
        window!.Close();
    }
}