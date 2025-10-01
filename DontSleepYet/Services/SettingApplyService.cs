using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Common;
using DontSleepYet.Contracts.Services;
using DontSleepYet.Models;

namespace DontSleepYet.Services;
public class SettingApplyService
{
    #region Properties.
    public bool Enabled { get; set; } = true;
    #endregion Properties.

    #region Services.
    private readonly IDontSleepService dontSleepService;
    private readonly IKeyHookService   keyHookService;
    LocalSettingsOptions localSettings;
    #endregion Services.

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="dontSleepService"></param>
    /// <param name="keyHookService"></param>
    public SettingApplyService(LocalSettingsOptions options,
                    IDontSleepService dontSleepService,
                    IKeyHookService keyHookService)
    {
        options.PropertyChanged += Options_PropertyChanged;

        this.dontSleepService = dontSleepService;
        this.keyHookService   = keyHookService;

        this.localSettings    = options;
    }

    public async Task ApplyServicesAsync()
    {
        if(dontSleepService.IsActive != localSettings.IsDontSleepActive)
        {
            if (localSettings.IsDontSleepActive)
            {
                await dontSleepService.StartAsync();
            }
            else
            {
                await dontSleepService.StopAsync();
            }
        }

        if (keyHookService.IsStarted != localSettings.IsEnabledKeyHookForAudioControl)
        {
            if (localSettings.IsDontSleepActive)
            {
                keyHookService.Start();
            }
            else
            {
                keyHookService.Stop();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Options_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (!Enabled)
        {
            return;
        }

        switch (e.PropertyName!)
        {
            case nameof(LocalSettingsOptions.IsDontSleepActive):
                {
                    var enabled = sender!.GetType().GetProperty(e.PropertyName!)!.GetValue(sender) as bool?;
                    if (enabled is null)
                    {
                        return;
                    }

                    if (enabled.Value)
                    {
                        await dontSleepService.StartAsync();
                    }
                    else
                    {
                        await dontSleepService.StopAsync();
                    }
                }

                break;
            case nameof(LocalSettingsOptions.IsEnabledKeyHookForAudioControl):
                {
                    var enabled = sender!.GetType().GetProperty(e.PropertyName!)!.GetValue(sender) as bool?;
                    if (enabled is null)
                    {
                        return;
                    }

                    if (enabled.Value)
                    {
                        keyHookService.Start();
                    }
                    else
                    {
                        keyHookService.Stop();
                    }
                }

                break;
        }
    }    
}
