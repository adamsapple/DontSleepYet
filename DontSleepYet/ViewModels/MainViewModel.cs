using System.Diagnostics;
using System.Reflection;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DontSleepYet.Contracts.Services;
using DontSleepYet.Helpers;
using Microsoft.UI.Xaml;
using DispatchQueue = Microsoft.UI.Dispatching.DispatcherQueue;

namespace DontSleepYet.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    private readonly ILocalSettingsService localSettingsService;
    private readonly IDontSleepService dontSleepService;
    private readonly ISystemInfoLiteService systemInfoLiteService;

    readonly string APP_DESCRIPTION = "自動ログオフ抑止(DontSleepYet)";

    private bool isDontSleepActive = false;

    /// <summary>
    /// 自動ログオフを抑止するかどうか
    /// </summary>
    public bool IsDontSleepActive
    {
        get => isDontSleepActive;
        set
        {
            if (isDontSleepActive == value)
            {
                return;
            }
            SetProperty(ref isDontSleepActive, value);

            dontSleepService.IsActive = isDontSleepActive;
            localSettingsService.SaveSettingAsync(nameof(IsDontSleepActive), IsDontSleepActive);
        }
    }

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
    private double _cpuUsage = 0.0;

    [ObservableProperty]
    private double _memUsage = 0.0;

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

        IsDontSleepActive = await localSettingsService.ReadSettingAsync<bool>(nameof(IsDontSleepActive));

        isInitialized = true;
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

    public MainViewModel(ILocalSettingsService localSettingsService, IDontSleepService dontSleepService, ISystemInfoLiteService systemInfoLiteService)
    {
        this.localSettingsService  = localSettingsService;
        this.dontSleepService      = dontSleepService;
        this.systemInfoLiteService = systemInfoLiteService;

        // Initialize the service state
        dontSleepService.Initialize();

        //IsDontSleepActive = dontSleepService.IsActive;
        IsRegistStartUp = StartUpHelper.ExistsStartUp_CurrentUserRun(APP_DESCRIPTION);

        
        systemInfoLiteService.Initialize();

        systemInfoLiteService.OnSystemInfoUpdated += SystemInfoLiteService_OnSystemInfoUpdated;
        CpuUsage = 0.0f;

        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
    }

    private readonly DispatchQueue dispatcherQueue;

    private void SystemInfoLiteService_OnSystemInfoUpdated(float cpuUsage, float memUsage)
    {
        

        //var dispatcherQueue = App.GetService<Window>().DispatcherQueue;
        dispatcherQueue.TryEnqueue(() =>
            {
                CpuUsage = cpuUsage;
                MemUsage = memUsage;
            });
    }
}
