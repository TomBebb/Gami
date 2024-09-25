using System;
using System.Text.Json;
using Avalonia.Controls;
using Gami.Desktop.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace Gami.Desktop.ViewModels;

public class MainViewModel : ViewModelBase
{
    private string _curr = "Library";

    public MainViewModel()
    {
        Log.Debug("Curr: {Curr}", Curr);
        CurrView = new LibraryView { DataContext = (LibraryViewModel)CurrObject };
    }

    public string Curr
    {
        get => _curr;
        set
        {
            if (_curr == value)
                return;
            _curr = value;

            Log.Debug("Curr: {Curr}", value);
            this.RaiseAndSetIfChanged(ref _curr, value);
            CurrObject = value switch
            {
                "Library" => new LibraryViewModel(),
                _ => null
            };
            Log.Debug("CurrObject: {Data}", JsonSerializer.Serialize(CurrObject, JsonSerializerOptions.Default));

            CurrView = value switch
            {
                "Library" => new LibraryView { DataContext = CurrObject },
                "Settings" => new SettingsView { DataContext = CurrObject },
                "Add-Ons" => new AddOnsView { DataContext = CurrObject },
                _ => throw new ApplicationException($"Invalid route: {value}")
            };
        }
    }

    [Reactive] public ReactiveObject? CurrObject { get; set; } = new LibraryViewModel();

    [Reactive] public UserControl? CurrView { get; set; }
}