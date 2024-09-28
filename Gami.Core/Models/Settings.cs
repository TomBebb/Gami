using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Gami.Core.Models;

public class Settings : ReactiveObject
{
    [Reactive] public string? DefaultProton { get; set; }
}