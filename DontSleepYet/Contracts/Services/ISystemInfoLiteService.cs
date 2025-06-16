using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontSleepYet.Contracts.Services;
/// <summary>
/// 
/// </summary>
public interface ISystemInfoLiteService
{
    float CpuUsage { get; }            /// CPU使用率
    long PhisicalMemory { get; }       /// 総メモリ量
    long TotalMemory { get; }          /// 物理+Swapメモリ量
    long PhisicalMemoryUsage { get; }  /// 物理メモリ使用量
    long SwapMemoryUsage { get; }      /// Swapメモリ使用量
    int RefleshRate                    /// 更新間隔(ミリ秒)
    { 
        get; 
        set;
    } 

    Task InitializeAsync();           /// 初期化メソッド
    void Initialize();

    Task StartAsync();                /// 初期化メソッド
    void Stop();

    public event SystemInfoUpdated OnSystemInfoUpdated; /// CPU使用率更新イベント
}

public delegate void SystemInfoUpdated(float cpuUsage, float memUsage);