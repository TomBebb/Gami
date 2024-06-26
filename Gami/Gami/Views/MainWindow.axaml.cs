using System;
using System.Text.Json;
using Avalonia.Controls;
using Gami.Scanners;

namespace Gami.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        Console.WriteLine("Built-in apps: " +
                          JsonSerializer.Serialize(BuiltInAppScanner.ScanApps(), SerializerSettings.JsonOptions));
        InitializeComponent();
    }
}