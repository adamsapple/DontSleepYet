using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Networking.NetworkOperators;
using static Mobilespot.MobileHotspotController;

namespace Mobilespot;

/// <summary>
/// 
/// </summary>
public class MobileHotspotController
{
    /// <summary>
    /// 
    /// </summary>
    public void LaunchConfig()
    {
        var uri = new Uri("ms-settings:network-mobilehotspot");
        Task.Run(async () =>
        {
            await Windows.System.Launcher.LaunchUriAsync(uri);
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsTetheringSupported()
    {
        var profile = NetworkInformation.GetInternetConnectionProfile();
        if (profile == null)
        {
            return false;
        }
        var tetheringManager = NetworkOperatorTetheringManager.CreateFromConnectionProfile(profile);
        
        return (tetheringManager != null);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsTetheringEnabled()
    {
        // 現在のインターネット接続プロファイルを取得
        var profile = NetworkInformation.GetInternetConnectionProfile();

        if (profile == null)
        {
            Debug.WriteLine("インターネット接続プロファイルが見つかりません。");
            return false;
        }

        // TetheringManager を作成
        var tetheringManager = NetworkOperatorTetheringManager.CreateFromConnectionProfile(profile);

        // 状態を取得
        var state = tetheringManager.TetheringOperationalState;

        Debug.WriteLine($"テザリング状態: {state.ToString()}");

        return state == (TetheringOperationalState.On);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task StartTetheringAsync()
    {
        // 現在のインターネット接続プロファイルを取得
        var profile = NetworkInformation.GetInternetConnectionProfile();

        if (profile == null)
        {
            Debug.WriteLine("インターネット接続プロファイルが見つかりません。");
            return;
        }

        // TetheringManager を作成
        var tetheringManager = NetworkOperatorTetheringManager.CreateFromConnectionProfile(profile);

        // 状態を取得
        var state = tetheringManager.TetheringOperationalState;

        if (state != TetheringOperationalState.Off)
        {
            return;
        }

        var result = await tetheringManager.StartTetheringAsync();

        Debug.WriteLine($"テザリング開始結果: {result.Status}");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task StopTetheringAsync()
    {
        // 現在のインターネット接続プロファイルを取得
        var profile = NetworkInformation.GetInternetConnectionProfile();

        if (profile == null)
        {
            Debug.WriteLine("インターネット接続プロファイルが見つかりません。");
            return;
        }

        // TetheringManager を作成
        var tetheringManager = NetworkOperatorTetheringManager.CreateFromConnectionProfile(profile);

        // 状態を取得
        var state = tetheringManager.TetheringOperationalState;

        if (state != TetheringOperationalState.On)
        {
            return;
        }

        var result = await tetheringManager.StopTetheringAsync();

        Debug.WriteLine($"テザリング停止結果: {result.Status}");
    }
}
