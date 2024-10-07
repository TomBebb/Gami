using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;
using Avalonia.Styling;
using ReactiveUI;
using TheArtOfDev.HtmlRenderer.Avalonia;

namespace Gami.Desktop.Views;

public sealed class AutoColorHtmlLabel: UserControl
{
    public static readonly AvaloniaProperty<string> TextProperty =
        AvaloniaProperty.Register<AutoColorHtmlLabel, string>(nameof(Text), "");
    [Content]
    public string Text
    {
        get => (string)GetValue(TextProperty)!;
        set => SetValue(TextProperty, value);
    }

    private readonly HtmlLabel _htmlLabel = new(){Text = "<div>Hello World!</div>"};

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TextProperty)
            UpdateInner();
    }

    private string HtmlContent
    {
        get => _htmlLabel.Text;
        set => _htmlLabel.Text = value;
    }
    public AutoColorHtmlLabel()
    {
        Content = _htmlLabel;
        UpdateInner();
        Application.Current!.WhenAnyValue(v =>v.ActualThemeVariant).Subscribe((_) => UpdateInner());
    }
    
    private void UpdateInner()
    {
        var theme = Application.Current!.ActualThemeVariant;
        HtmlContent = theme == ThemeVariant.Dark ?  $"<div style=\"color: white\">{Text}</div>": Text;
    }
}