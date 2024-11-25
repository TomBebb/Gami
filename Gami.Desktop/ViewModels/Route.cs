using System;
using FluentAvalonia.UI.Controls;

namespace Gami.Desktop.ViewModels;

public sealed record Route(
    string Path,
    string Name,
    Func<string, ViewModelBase> ViewModelFactory,
    string Tooltip = "",
    Symbol? Icon = null);