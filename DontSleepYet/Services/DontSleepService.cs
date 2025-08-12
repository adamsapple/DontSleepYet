using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using DontSleepYet.Contracts.Services;
using Microsoft.UI.Xaml;
using Microsoft.Win32;
using Windows.System;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace DontSleepYet.Services;

public class DontSleepService : IDontSleepService
{
    private bool _isActive; // Backing field for IsActive property

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive == value)
            {
                return;
            }

            if (value)
            {
                // Start the service to prevent the system from sleeping
                StartAsync().GetAwaiter().GetResult();
            }
            else
            {
                // Stop the service to allow the system to sleep again
                StopAsync().GetAwaiter().GetResult();
            }

            _isActive = value; // Update the backing field
        }
    }

    private TimeSpan wakeUpDuration = TimeSpan.FromSeconds(5); // Default to 5 seconds
    public int WakeUpDurationSeconds
    {
        get => (int)wakeUpDuration.TotalSeconds;
        
        set
        {
            wakeUpDuration = TimeSpan.FromSeconds(value);

            _timer.Interval = wakeUpDuration.TotalMilliseconds;
        }
    }

    private bool _isInitialized = false;

    public Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return Task.CompletedTask;
        }

        WakeUpDurationSeconds = 5;
        _timer.Elapsed += OnTimerElapsed;

        _isInitialized = true;
        return Task.CompletedTask;
    }

    public void Initialize()
    {
        InitializeAsync().GetAwaiter().GetResult();
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        var plan = "no-plan";
        uint result = 0;
        /*// 作戦1: マウスカーソルを(0, 0)移動する
        {
            plan = "作戦1";
            System.Drawing.Point cursorPos;
            Windows.Win32.PInvoke.GetCursorPos(out cursorPos);
            Windows.Win32.PInvoke.SetCursorPos(cursorPos.X-1, cursorPos.Y); // Move the cursor to (0, 0) to prevent sleep
            //Windows.Win32.PInvoke.SetCursorPos(cursorPos.X, cursorPos.Y);   // Move the cursor to (0, 0) to prevent sleep
        }//*/

        /*// 作戦2: F15キーを押下する
        {
            plan = "作戦2";
            Span<INPUT> inputs = stackalloc INPUT[2];

            inputs[0].type = INPUT_TYPE.INPUT_KEYBOARD;
            inputs[0].Anonymous.ki.wVk = VIRTUAL_KEY.VK_F15;

            inputs[1].type = INPUT_TYPE.INPUT_KEYBOARD;
            inputs[1].Anonymous.ki.wVk = VIRTUAL_KEY.VK_F15;
            inputs[1].Anonymous.ki.dwFlags = KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP;

            Windows.Win32.PInvoke.SendInput(inputs, Marshal.SizeOf<INPUT>());
        }//*/

        /// 作戦3: マウスカーソルを(0, 0)移動する
        {
            plan = "作戦3";
            Span<INPUT> inputs = stackalloc INPUT[2];
            int mx = 5;
            
            inputs[0].type = INPUT_TYPE.INPUT_MOUSE;
            inputs[0].Anonymous.mi.dwFlags = MOUSE_EVENT_FLAGS.MOUSEEVENTF_MOVE;
            inputs[0].Anonymous.mi.dx = -mx;
            inputs[0].Anonymous.mi.dy = 0;
            inputs[0].Anonymous.mi.mouseData = 0; // No additional data
            inputs[0].Anonymous.mi.time = 0;      // Use the current time
            inputs[0].Anonymous.mi.dwExtraInfo = (nuint)Windows.Win32.PInvoke.GetMessageExtraInfo().Value;

            inputs[1].type = INPUT_TYPE.INPUT_MOUSE;
            inputs[1].Anonymous.mi.dwFlags = MOUSE_EVENT_FLAGS.MOUSEEVENTF_MOVE;
            inputs[1].Anonymous.mi.dx = mx;
            inputs[1].Anonymous.mi.dy = 0;
            inputs[1].Anonymous.mi.mouseData = 0; // No additional data
            inputs[1].Anonymous.mi.time = 0;      // Use the current time
            inputs[1].Anonymous.mi.dwExtraInfo = (nuint)Windows.Win32.PInvoke.GetMessageExtraInfo().Value;
            
            result = Windows.Win32.PInvoke.SendInput(inputs, Marshal.SizeOf<INPUT>() * 2);

            //inputs[0].Anonymous.mi.dx = mx;
            
            //result = Windows.Win32.PInvoke.SendInput(inputs, Marshal.SizeOf<INPUT>());
        }//*/

        Debug.WriteLine($"Dontsleep.OnTimerElapsed( {plan} ): = {result}, {DateTime.Now}");

    }

    public Task StartAsync()
    {
        if (_timer.Enabled)
        {
            return Task.CompletedTask;
        }

        _timer.Start();

        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        if (!_timer.Enabled)
        {
            _timer.Stop();
        }

        return Task.CompletedTask;
    }

    private readonly System.Timers.Timer _timer = new();
}

