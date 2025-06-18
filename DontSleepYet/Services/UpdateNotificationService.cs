using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Common;
using DontSleepYet.Contracts.Services;
using DontSleepYet.Helpers;
using Microsoft.UI.Dispatching;

namespace DontSleepYet.Services;
public class UpdateNotificationService : IUpdateNotificationService
{
#region Services.
    private IUpdateCheckService updateCheckService;
    private ILocalSettingsService localSettingsService;
    private IAppNotificationService appNotificationService;
#endregion Services.

    private readonly DispatcherQueue dispatcherQueue;
    private CancellationTokenSource? cts;

    private UpdateCheckData? latestCheckData;
    private DateTimeOffset ignorePublishedAt = DateTimeOffset.MinValue;
    private string ignoreVersion = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task StartAsync()
    {
        if (IsEnable)
        {
            return;
        }

        IsEnable = true;
        cts = new CancellationTokenSource();

        await InitializeAsync();

        while (IsEnable)
        {
            var now = DateTime.Now;
            var duration = CheckPeriod - (now - LastCheckedAt);

            if(duration.TotalSeconds > 0)
            {
                await Task.Delay(duration, cts.Token);
            }

            if (!IsEnable)
            {
                break;
            }

            var updateCheckData = await updateCheckService.CheckUpdateAsync();

            if (updateCheckData.IsCheckSuccess)
            {
                LastCheckedAt = now;

                if(updateCheckData.IsUpdateAvailable
                   && ignorePublishedAt < updateCheckData.PublishedAt)
                {
                    Debug.WriteLine($"Update available: {updateCheckData.LatestVersion}");
                    Debug.WriteLine($"More info: {updateCheckData.InfoUrl}");

                    ShowNotification(string.Format("UpdateNotificationPayload".GetLocalized(), updateCheckData.InfoUrl, updateCheckData.LatestVersion));
                }

                latestCheckData = updateCheckData;
            }
            else
            {
                /// チェック失敗時は、1時間待機してみる
                await Task.Delay(TimeSpan.FromHours(1), cts.Token);
            }
        }

        cts.Dispose();
        cts = null;
    }

    public void Stop()
    {
        IsEnable = false;
        cts?.Cancel();
    }

    public void IgnoreThisVersion()
    {
        /// 最新のチェックデータがない、または更新がない場合は何もしない
        if (latestCheckData == null
            || !latestCheckData.IsCheckSuccess || !latestCheckData.IsUpdateAvailable) 
        {
            return;
        }

        /// 今回のversionは無視するように設定する
        Task.Run(async () =>
        {
            // Debug.WriteLine($"Ignore this version: {latestCheckData.LatestVersion}");

            await localSettingsService.SaveSettingAsync("UpdateCheck.Ignore.Version", latestCheckData.LatestVersion);
            await localSettingsService.SaveSettingAsync("UpdateCheck.Ignore.PublishedAt", latestCheckData.PublishedAt);
        }).GetResultOrDefault();
    }


    public void TestShow()
    {
        var payload = string.Format("UpdateNotificationPayload".GetLocalized(), "https://www.google.com");
        ShowNotification(payload);
    }

    private async Task InitializeAsync()
    {
        if (isInitialized)
        {
            return;
        }

#if DEBUG
        await localSettingsService.SaveSettingAsync("UpdateCheck.LastCheckedAt", DateTime.MinValue); // test code/
#endif

        LastCheckedAt     = await localSettingsService.ReadSettingAsync<DateTime>("UpdateCheck.LastCheckedAt", DateTime.MinValue);
        CheckPeriod       = await localSettingsService.ReadSettingAsync<TimeSpan>("UpdateCheck.CheckPeriod", TimeSpan.FromDays(1));

        ignoreVersion     = await localSettingsService.ReadSettingAsync<string>("UpdateCheck.Ignore.Version") ?? string.Empty;
        ignorePublishedAt = await localSettingsService.ReadSettingAsync<DateTimeOffset>("UpdateCheck.Ignore.PublishedAt");

        isInitialized = true;
    }

    public bool IsEnable { get; private set; } = false;

    private bool isInitialized = false;

    private DateTime _lastCheckedAt = DateTime.MinValue;
    public DateTime LastCheckedAt {
        get => _lastCheckedAt;
        private set
        {
            if(_lastCheckedAt == value)
            {
                return;
            }

            Task.Run(() => {
                localSettingsService.SaveSettingAsync("UpdateCheck.LastCheckedAt", LastCheckedAt).GetResultOrDefault();
            });

            _lastCheckedAt = value;
        }
    }

    private TimeSpan _checkPeriod = TimeSpan.FromDays(1);
    public TimeSpan CheckPeriod { 
        get => _checkPeriod;
        set => _checkPeriod = value;
    }

    private void ShowNotification(string payload)
    {
        dispatcherQueue.TryEnqueue(() => appNotificationService.Show(payload));
    }

    public UpdateNotificationService(IUpdateCheckService updateCheckService,
                                     ILocalSettingsService localSettingsService,
                                     IAppNotificationService appNotificationService)
    {
        this.updateCheckService     = updateCheckService;
        this.localSettingsService   = localSettingsService;
        this.appNotificationService = appNotificationService;

        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
    }
}
