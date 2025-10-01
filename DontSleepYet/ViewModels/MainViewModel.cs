using System.Diagnostics;
using System.Reflection;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.Input;
using DontSleepYet.Contracts.Services;
using DontSleepYet.Helpers;
using DontSleepYet.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using static System.Runtime.InteropServices.JavaScript.JSType;
using DispatchQueue = Microsoft.UI.Dispatching.DispatcherQueue;

namespace DontSleepYet.ViewModels;

public partial class MainViewModel : ObservableValidator//ObservableRecipient
{
    #region Services.
    private readonly ILocalSettingsService localSettingsService;
    private readonly IDontSleepService dontSleepService;
    private readonly ISystemInfoLiteService systemInfoLiteService;
    private readonly IUpdateCheckService updateCheckService;
    private readonly IUpdateService updateService;
    #endregion Services.

    private readonly string APP_DESCRIPTION;

    private bool isRegistStartUp = false;

    /// <summary>
    /// スタートップに登録するかどうか
    /// </summary>
    public bool IsRegistStartUp
    {
        get => isRegistStartUp;
        set
        {
            if (isRegistStartUp == value)
            {
                return;
            }
            SetProperty(ref isRegistStartUp, value);

            if (isRegistStartUp)
            {
                if (StartUpHelper.ExistsStartUp_CurrentUserRun(APP_DESCRIPTION))
                {
                    return;
                }

                StartUpHelper.RegiserStartUp_CurrentUserRun(APP_DESCRIPTION);
            }
            else
            {
                StartUpHelper.UnregiserStartUp_CurrentUserRun(APP_DESCRIPTION);
            }
        }
    }


    [ObservableProperty]
    [Range(5, 119, ErrorMessage = "範囲外です")]
    private int _dontSleepDurationSeconds = 30;

    partial void OnDontSleepDurationSecondsChanged(int value)
    {
        if(int.IsNegative(value) || dontSleepService.WakeUpDurationSeconds == value)
        {
            return;
        }

        LocalSettingsOptions.DontSleepWakeUpDurationSeconds = value;
    }

    [ObservableProperty]
    private double _cpuUsage = 0.0;

    [ObservableProperty]
    private double _memUsage = 0.0;

    [ObservableProperty]
    private DateTime? _updateCheckedAt = null;

    public LocalSettingsOptions LocalSettingsOptions { get; }


    private bool isInitialized = false;
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task InitializeAsync()
    {
        if (isInitialized)
        {
            return;
        }

        isInitialized = true;
    }


    [RelayCommand]
    private async void Update()
    {
        var data = await updateCheckService.CheckUpdateAsync();
    

        if (data == null)// || !data.IsUpdateAvailable)
        {
            return;
        }

        await updateService.UpdateAsync(data.ArchiveUrl!);

    }

    [RelayCommand]
    private async void LaunchTetheringConfig()
    {
        var controller = new Mobilespot.MobileHotspotController();
        controller.LaunchConfig();
    }
    //[RelayCommand]
    //private async Task SaveWindowPosition()
    //{
    //    var window = App.MainWindow;
    //    if (window == null)
    //    {
    //        return;
    //    }
    //    // Save the current position and size of the window
    //    await localSettingsService.SaveSettingAsync("WindowPosition.X", window.AppWindow.Position.X);
    //    await localSettingsService.SaveSettingAsync("WindowPosition.Y", window.AppWindow.Position.Y);
    //}

    /// <summary>
    /// CPU使用率
    /// </summary>
    //public double CpuUsage
    //{
    //    get => _cpuUsage;
    //    set => SetProperty(ref _cpuUsage, value);
    //}

    public MainViewModel(ILocalSettingsService localSettingsService,
                        IDontSleepService dontSleepService,
                        ISystemInfoLiteService systemInfoLiteService,
                        IUpdateCheckService updateCheckService,
                        IUpdateService updateService,
                        LocalSettingsOptions localSettingsOptions)
    {
        this.localSettingsService  = localSettingsService;
        this.dontSleepService      = dontSleepService;
        this.systemInfoLiteService = systemInfoLiteService;
        this.updateService         = updateService;
        this.updateCheckService    = updateCheckService;
        this.LocalSettingsOptions  = localSettingsOptions;


        APP_DESCRIPTION = nameof(APP_DESCRIPTION).GetLocalized();

        DontSleepDurationSeconds = localSettingsOptions.DontSleepWakeUpDurationSeconds;

        //IsDontSleepActive = dontSleepService.IsActive;
        IsRegistStartUp = StartUpHelper.ExistsStartUp_CurrentUserRun(APP_DESCRIPTION);

        
        systemInfoLiteService.Initialize();

        systemInfoLiteService.OnSystemInfoUpdated += SystemInfoLiteService_OnSystemInfoUpdated;
        CpuUsage = 0.0f;

        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();


        //IsEnabledKeyHookForAudioControl = localSettingsOptions.IsEnabledKeyHookForAudioControl;
    }

    private readonly DispatchQueue dispatcherQueue;

    private void SystemInfoLiteService_OnSystemInfoUpdated(float cpuUsage, float memUsage)
    {
        //var dateTime = Task.Run(async () =>
        //{
        //    return await localSettingsService.ReadSettingAsync<DateTime>("UpdateCheck.LastCheckedAt", DateTime.MinValue);
        //}).Result;

        var dateTime = LocalSettingsOptions.UpdateCheck_LastCheckedAt;

        //var dispatcherQueue = App.GetService<Window>().DispatcherQueue;
        dispatcherQueue.TryEnqueue(() =>
            {
                CpuUsage = cpuUsage;
                MemUsage = memUsage;

                UpdateCheckedAt = dateTime;
            });
    }
}
