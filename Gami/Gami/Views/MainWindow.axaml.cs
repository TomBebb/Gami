using System;
using System.Collections.Immutable;
using System.Text.Json;
using Avalonia.Controls;
using Gami.Scanners;

namespace Gami.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        var steamScanner = new SteamScanner();
        var fetched = steamScanner.Scan().ToImmutableList();
        Console.WriteLine($"Steam apps {JsonSerializer.Serialize(fetched, SerializerSettings.JsonOptions)}");
        InitializeComponent();
    }
}