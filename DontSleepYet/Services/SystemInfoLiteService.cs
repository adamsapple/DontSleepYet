using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using DontSleepYet.Contracts.Services;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System;
using Windows.System.Diagnostics;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;


namespace DontSleepYet.Services;
internal class SystemInfoLiteService : ISystemInfoLiteService
{
    public float CpuUsage => 0F;

    public long PhisicalMemory => 0L;

    public long TotalMemory => 0L;

    public long PhisicalMemoryUsage => 0L;

    public long SwapMemoryUsage => 0L;


    private TimeSpan refleshDuration = TimeSpan.FromSeconds(3);
    public int RefleshRate
    {
        get => (int)refleshDuration.TotalSeconds;
        set
        {
            refleshDuration = TimeSpan.FromSeconds(value);
            _timer.Interval = refleshDuration.TotalMilliseconds;
        }
    }

    private readonly System.Timers.Timer _timer = new();
    private bool _isInitialized = false;

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。'required' 修飾子を追加するか、Null 許容として宣言することを検討してください。
    public event SystemInfoUpdated OnSystemInfoUpdated;
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。'required' 修飾子を追加するか、Null 許容として宣言することを検討してください。

    public Task InitializeAsync() 
    {
        if (_isInitialized)
        {
            return Task.CompletedTask;
        }

        RefleshRate = 2;

        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();

        _isInitialized = true;


        return Task.CompletedTask;
    }
    
    //private DispatcherQueue dispatcherQueue;

    public void Initialize()
    {
        InitializeAsync().GetAwaiter().GetResult();
    }

    private SystemCpuUsageReport? prevCpuReport = null;

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        var systemInfo = SystemDiagnosticInfo.GetForCurrentSystem();
        var cpuUsage = systemInfo.CpuUsage.GetReport();
        var memUsage = systemInfo.MemoryUsage.GetReport();

        if (prevCpuReport == null)
        {
            prevCpuReport = cpuUsage;
            return;
        }        

        var kernel = cpuUsage.KernelTime - prevCpuReport.KernelTime;
        var user   = cpuUsage.UserTime - prevCpuReport.UserTime;
        var idle   = cpuUsage.IdleTime - prevCpuReport.IdleTime;
        var kpu    = kernel + user;
        
        var total  = kernel + user + idle;

        //Debug.WriteLine($"kernel: {kernel.TotalSeconds}");
        //Debug.WriteLine($"user  : {user.TotalSeconds}");
        //Debug.WriteLine($"idle  : {idle.TotalSeconds}");
        //Debug.WriteLine($"k+u   : {(kernel + user).TotalSeconds}");
        //Debug.WriteLine($"i+u   : {(idle + user).TotalSeconds}");
        //Debug.WriteLine($"total : {total.TotalSeconds}");

        prevCpuReport = cpuUsage;
        //var result = 0.45f;

        /// CPU使用率を計算する：各値の使い方がよくわかってないが、こんな気がする
        if (kpu.TotalSeconds != 0)
        {
            var cpu_usage = (kpu.TotalSeconds - idle.TotalSeconds) / kpu.TotalSeconds;

            var mem_usage = 1.0 - (memUsage.AvailableSizeInBytes * 1.0 / memUsage.TotalPhysicalSizeInBytes);

            OnSystemInfoUpdated?.Invoke((float)cpu_usage, (float)mem_usage);
        }

        var swap_reserve = memUsage.CommittedSizeInBytes - memUsage.TotalPhysicalSizeInBytes;
        var phis_using   = memUsage.TotalPhysicalSizeInBytes - memUsage.AvailableSizeInBytes;
        //var commit_using
        //var commit_total
        //Debug.WriteLine($"phisical.Total: {memUsage.TotalPhysicalSizeInBytes / 1024.0 / 1024 / 1024 }");
        //Debug.WriteLine($"phisical.Using: {phis_using / 1024.0 / 1024 / 1024}");
        //Debug.WriteLine($"phisical.Free : {memUsage.AvailableSizeInBytes / 1024.0 / 1024 / 1024}");
        //Debug.WriteLine($"virtua.Reserve: {memUsage.CommittedSizeInBytes / 1024.0 / 1024 / 1024}");
        //Debug.WriteLine($"swap.Reserve  : {swap_reserve / 1024.0 / 1024 / 1024}");
        //Debug.WriteLine($"user  : {user.TotalSeconds}");
        //Debug.WriteLine($"idle  : {idle.TotalSeconds}");
        //Debug.WriteLine($"k+u   : {(kernel + user).TotalSeconds}");
        //Debug.WriteLine($"i+u   : {(idle + user).TotalSeconds}");
        //Debug.WriteLine($"total : {total.TotalSeconds}");



    }

    public async Task StartAsync()
    {
        if (_timer.Enabled)
        {
            return;
        }

        await InitializeAsync();

        _timer.Start();
    }

    public void Stop() 
    {
        if (!_timer.Enabled)
        {
            return;
        }

        _timer.Stop();
    }
}
