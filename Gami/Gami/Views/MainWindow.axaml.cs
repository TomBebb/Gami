using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Gami.Db.Schema.Metadata;
using Gami.Scanners;

namespace Gami.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        var steamScanner = new SteamScanner();

        var fetched = DoScan(steamScanner).GetAwaiter().GetResult();
        Console.WriteLine($"Steam apps {JsonSerializer.Serialize(fetched, SerializerSettings.JsonOptions)}");
        InitializeComponent();
    }

    private static async ValueTask<List<IGameLibraryRef>> DoScan(SteamScanner scanner)
    {
        var list = new List<IGameLibraryRef>();
        await foreach (var item in scanner.Scan()) list.Add(item);
        return list;
    }
}