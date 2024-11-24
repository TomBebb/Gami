using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using Gami.Desktop.ViewModels;
using Serilog;

namespace Gami.Desktop.Views;

public partial class MainView : UserControl
{
    private Window Window => (Window)Parent!;

    private WindowState WindowState
    {
        get => Window.WindowState;
        set => Window.WindowState = value;
    }

    public MainView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (WindowState == WindowState.Normal)
            Window.BeginMoveDrag(e);
    }

    private void NavigationView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        Log.Debug("NavigationView_OnSelectionChanged: {Selection}", e.SelectedItem);
        (DataContext as MainViewModel)!.CurrRoute = (Route)e.SelectedItem;
    }

    private void Minimize(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Maximize(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void Close(object? sender, RoutedEventArgs e) => Window.Close();
}