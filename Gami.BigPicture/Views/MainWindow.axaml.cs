using Avalonia.Controls;
using Gami.BigPicture.Inputs;

namespace Gami.BigPicture.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        KeyDown += InputManager.Instance.OnKeyDown;
        KeyUp += InputManager.Instance.OnKeyUp;
        InitializeComponent();
    }
}