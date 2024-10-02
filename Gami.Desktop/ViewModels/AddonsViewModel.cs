using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using AvaloniaWebView;
using FluentAvalonia.UI.Controls;
using Gami.Core;
using Gami.Desktop.Plugins;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Gami.Desktop.ViewModels;

public sealed class AddonsViewModel : ViewModelBase
{
    public AddonsViewModel()
    {
        Log.Information("Installed addons: {Addons}", Installed.Select(addon => addon.Name));

        SelectedAddon = Installed.First();

        this.WhenAnyValue(v => v.SelectedAddon)
            .Subscribe(v =>
            {
                Auth = v == null ? null :
                    GameExtensions.LibraryAuthByName.TryGetValue(v.Key, out var auth) ? auth : null;
            });

        this.WhenAnyValue(v => v.Auth)
            .Select(v => v?.AuthUrl())
            .BindTo(this, v => v.InitialUrl);
        this.WhenAnyValue(v => v.CurrentUrl)
            .Subscribe(v => Log.Debug("URL changed: {Url}", v));

        ReAuthCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (InitialUrl != null)
                CurrentUrl = InitialUrl;
            if (CurrentUrl == null)
                return;
            var webview = new WebView { Url = new Uri(CurrentUrl!), MinHeight = 400, MinWidth = 400 };
            webview.NavigationStarting += (_, arg) => CurrentUrl = arg.Url?.ToString() ?? CurrentUrl;
            var dialog = new ContentDialog
            {
                Title = "My Dialog Title",
                CloseButtonText = "Close",
                Content = webview
            };
            await dialog.ShowAsync();

            this.WhenAnyValue(v => v.CurrentUrl)
                .Subscribe(OnUrlChange);
            return;

            async void OnUrlChange(string? v)
            {
                Log.Debug("weebbview URL changed: {Url}", v);
                if (v == null || Auth == null) return;
                if (await Auth!.CurrUrlChange(v)) dialog.Hide(ContentDialogResult.Primary);
            }
        });
    }

    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    [Reactive] private string? InitialUrl { get; set; }
    [Reactive] private string? CurrentUrl { get; set; }
    public ReactiveCommand<Unit, Unit> ReAuthCommand { get; set; }
#pragma warning disable CA1822
    public ImmutableArray<PluginConfig> Installed => GameExtensions.PluginConfigs.Values;
#pragma warning restore CA1822

    [Reactive] public PluginConfig SelectedAddon { get; set; }


#pragma warning disable CA1859
    [Reactive] private IGameLibraryAuth? Auth { get; set; }
#pragma warning restore CA1859
}