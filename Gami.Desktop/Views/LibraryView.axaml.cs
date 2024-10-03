using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Serilog;

namespace Gami.Desktop.Views;

public partial class LibraryView : UserControl
{
    public LibraryView()
    {
        InitializeComponent();

        var box = this.FindLogicalDescendantOfType<ListBox>();
        box.AddHandler(PointerPressedEvent, (sender, args) =>
        {
            Log.Information("Pressed PointerPressedEvent");
            var point = args.GetCurrentPoint(sender as Control);
            var props = point.Properties;
            Log.Information("Pressed PointerPressedEvent: {ty}",
                props.IsLeftButtonPressed ? "Left" : props.IsRightButtonPressed ? "Right" : "Middle");
            if (!props.IsLeftButtonPressed)
                args.Handled = true;
        }, RoutingStrategies.Tunnel);

        Log.Debug("Root Grid: {Grid}", box);
    }
}