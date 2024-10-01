using System.Collections.Generic;
using Avalonia.Controls;
using Gami.Core;
using Gami.Desktop.Plugins;
using Gami.Desktop.ViewModels;

namespace Gami.Desktop.Views;

public partial class AddOnsView : UserControl
{
    public AddOnsView()
    {
        InitializeComponent();
    }

    private void TextBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        var model = (MappedPluginConfigSetting)((TextBox)sender).DataContext;
        if (model == null) return;

        var addons = (AddonsViewModel)((TextBox)sender)?.Parent?.Parent?.Parent?.Parent?.DataContext;
        if (addons == null) return;
        var addon = addons.SelectedAddon;
        var curr = PluginJson.Load<Dictionary<string, object>>(addon.Key) ?? new Dictionary<string, object>();
        curr[model!.Key] = model!.Value;
        PluginJson.Save(curr, addon.Key);
    }
}