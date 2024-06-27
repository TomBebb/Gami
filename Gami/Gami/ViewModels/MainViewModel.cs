using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using EFCore.BulkExtensions;
using Gami.Db;
using Gami.Db.Schema.Metadata;
using ReactiveUI;
using System.Text.Json;

namespace Gami.ViewModels;

public class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        PlayGame = ReactiveCommand.Create((Game game) =>
        {
            Console.WriteLine("Play game: "+JsonSerializer.Serialize(game));
            game.Launch().AsTask().GetAwaiter().GetResult();
        });
    }
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Gami";
    public ReactiveCommand<Game, Unit> PlayGame { get; }


    public List<Game> Games
    {
        get
        {
            using var db = new GamiContext();
            return db.Games.ToList();
        }
        set
        {
            using var db = new GamiContext();
            db.BulkUpdateAsync(value);
        }
    }
#pragma warning restore CA1822 // Mark members as static
}