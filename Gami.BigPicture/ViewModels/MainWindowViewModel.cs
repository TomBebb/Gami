using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Gami.BigPicture.Views;
using ReactiveUI.Fody.Helpers;

namespace Gami.BigPicture.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        CurrView = new LibraryView { DataContext = CurrViewModel };

        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMinutes(1));
                CurrentTime.Add(TimeSpan.FromMinutes(1));
            }
        });
    }

    [Reactive] public TimeOnly CurrentTime { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

    [Reactive] public ViewModelBase? CurrViewModel { get; set; } = new LibraryViewModel();
    [Reactive] public UserControl? CurrView { get; set; }
}