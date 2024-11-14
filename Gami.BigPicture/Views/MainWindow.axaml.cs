using Avalonia.Controls;
using Avalonia.Threading;
using Gami.BigPicture.Inputs;
using Serilog;

namespace Gami.BigPicture.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        KeyDown += InputManager.Instance.OnKeyDown;
        KeyUp += InputManager.Instance.OnKeyUp;

        InputManager.Instance.OnPressed += btn =>
        {
            if (btn != MappedInputType.MainMenu || IsFocused) return;

            Dispatcher.UIThread.Post(() =>
            {
                WindowState = WindowState.Normal;
                WindowState = WindowState.FullScreen;
            });
            Log.Information("Attempt focus done");
        };
        InitializeComponent();

        Focus();
    }
}