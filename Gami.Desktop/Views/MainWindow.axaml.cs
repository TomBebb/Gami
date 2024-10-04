using Avalonia.Controls;
using FluentAvalonia.UI.Windowing;

namespace Gami.Desktop.Views;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        InitializeComponent();
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex; 
        
    }
}