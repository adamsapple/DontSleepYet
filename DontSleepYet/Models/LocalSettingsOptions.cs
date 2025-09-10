using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DontSleepYet.Models;

public partial class LocalSettingsOptions : ModelBase
{
    [ObservableProperty]
    private string? applicationDataFolder;
    

    [ObservableProperty]
    private string? localSettingsFile;
    

    [ObservableProperty]
    private int dontSleepWakeUpDurationSeconds;

    [ObservableProperty]
    private bool isDontSleepActive;

    public LocalSettingsOptions()
    {
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DontSleepWakeUpDurationSeconds))
        {
            DontSleepWakeUpDurationSeconds = DontSleepWakeUpDurationSeconds < 1 ? 1 : DontSleepWakeUpDurationSeconds;
        }
    }
}
