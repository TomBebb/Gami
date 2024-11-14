using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Gami.BigPicture.Views;
using Hardware.Info;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace Gami.BigPicture.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly HardwareInfo _hwInfo = new();

    public MainWindowViewModel()
    {
        CurrView = new LibraryView { DataContext = CurrViewModel };

        this.WhenAnyValue(v => v.BatteryState).Subscribe(bs =>
        {
            if (bs == null) return;

            Log.Information("Battery percent: {Percentage}, status: {Status}", bs.EstimatedChargeRemaining,
                bs.BatteryStatus);
            var roundedPercent = 10 * (int)Math.Round((float)bs.EstimatedChargeRemaining / 10);
            var percentEnding = roundedPercent == 100 ? "" : $"-{roundedPercent}";
            BatteryIcon = $"mdi-battery{percentEnding}";
        });

        Task.Run(async () =>
        {
            while (true)
            {
                _hwInfo.RefreshBatteryList();
                BatteryState = _hwInfo.BatteryList[0];
                await Task.Delay(TimeSpan.FromMinutes(1));
                CurrentTime.Add(TimeSpan.FromMinutes(1));
            }
        });
    }

    [Reactive] public Battery? BatteryState { get; set; }
    [Reactive] public string BatteryIcon { get; set; } = string.Empty;

    [Reactive] public TimeOnly CurrentTime { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

    [Reactive] public ViewModelBase? CurrViewModel { get; set; } = new LibraryViewModel();
    [Reactive] public UserControl? CurrView { get; set; }
}