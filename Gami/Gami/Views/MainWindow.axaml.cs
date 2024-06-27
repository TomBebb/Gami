using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using EFCore.BulkExtensions;
using Gami.Db;
using Gami.Db.Schema.Metadata;
using Gami.Scanners;

namespace Gami.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        var steamScanner = new SteamScanner();
        Console.WriteLine("Scan steam apps");
        var fetched = DoScan(steamScanner).GetAwaiter().GetResult();
        {
            using var db = new GamiContext();
            db.BulkInsert(fetched.Select(f => new Game
            {
                Name = f.Name,
                LibraryId = f.LibraryId,
                LibraryType = f.LibraryType,
                Description = ""
            }));
        }
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