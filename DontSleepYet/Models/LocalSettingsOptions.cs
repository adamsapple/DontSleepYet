using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DontSleepYet.Contracts.Services;
using DontSleepYet.Services;
using Windows.Graphics;

namespace DontSleepYet.Models;

public partial class LocalSettingsOptions : ModelBase
{
    #region Setting Properties.
    [ObservableProperty]
    private string? applicationDataFolder;

    [ObservableProperty]
    private string? localSettingsFile;

    [ObservableProperty]
    private int dontSleepWakeUpDurationSeconds;

    [ObservableProperty]
    private bool isDontSleepActive;

    [ObservableProperty]
    private bool isEnabledKeyHookForAudioControl;
    #endregion Setting Properties.

    #region state Properties.
    [ObservableProperty]
    private PointInt32 windowPotition = new(0, 0);

    [ObservableProperty]
    private string? updateCheck_Ignore_Version;

    [ObservableProperty]
    private DateTimeOffset updateCheck_Ignore_PublishedAt;

    [ObservableProperty]
    private DateTime updateCheck_LastCheckedAt;
    #endregion state Properties.




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

        //await _localSettingsService.SaveSettingAsync(e.PropertyName!, GetType().GetProperty(e.PropertyName!)!.GetValue(this));
    }

    //private readonly LocalSettingsService _localSettingsService;

    //public LocalSettingsOptions(LocalSettingsService localSettingsService)
    //{
    //    _localSettingsService = localSettingsService;
    //}
}
