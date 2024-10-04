using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using FluentAvalonia.UI.Controls;
using Gami.Core;
using Gami.Desktop.Addons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Xilium.CefGlue.Avalonia;

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
                    GamiAddons.LibraryAuthByName.TryGetValue(v.Key, out var auth) ? auth : null;
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

            var webview = new AvaloniaCefBrowser
            {
                Address = InitialUrl,
                MinWidth = 400,
                MinHeight = 200
            };
            webview.LoadStart += (_, ev) => CurrentUrl = ev.Frame.Url;
            var dialog = new ContentDialog
            {
                Title = "Authenticate",
                Content = webview,
                CloseButtonText = "Close",
                FullSizeDesired = true
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
    [Reactive] public string? InitialUrl { get; set; }
    [Reactive] public string? CurrentUrl { get; set; }
    public ReactiveCommand<Unit, Unit> ReAuthCommand { get; set; }
#pragma warning disable CA1822
    public ImmutableArray<AddonConfig> Installed => GamiAddons.AddonConfigs.Values;
#pragma warning restore CA1822

    [Reactive] public AddonConfig SelectedAddon { get; set; }


#pragma warning disable CA1859
    [Reactive] private IGameLibraryAuth? Auth { get; set; }
#pragma warning restore CA1859
}