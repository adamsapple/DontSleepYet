using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Certificates;
using Windows.System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;
using static System.Runtime.CompilerServices.RuntimeHelpers;


namespace DontSleepYet.Services;
public class KeyHookService : Contracts.Services.IKeyHookService
{
    private UnhookWindowsHookExSafeHandle? hookHandle = null;
    public bool IsStarted { get; private set; } = false;


    private void StartHook()
    {
        if (hookHandle != null)
        {
            return;
        }

        using (var process = Process.GetCurrentProcess())
        using (var module = process.MainModule)
        {
            // フックを行う
            // 第1引数	フックするイベントの種類
            //   13はキーボードフックを表す
            // 第2引数 フック時のメソッドのアドレス
            //   フックメソッドを登録する
            // 第3引数	インスタンスハンドル
            //   現在実行中のハンドルを渡す
            // 第4引数	スレッドID
            //   0を指定すると、すべてのスレッドでフックされる
            hookHandle = Windows.Win32.PInvoke.SetWindowsHookEx(
                                                WINDOWS_HOOK_ID.WH_KEYBOARD_LL, 
                                                HookCallback, 
                                                Windows.Win32.PInvoke.GetModuleHandle(module!.ModuleName), 
                                                0);
        }
    }

    enum KeyState
    {
        Down = 256,
        Up   = 257
    }

    private bool IsCtrlDown  = false;
    private bool IsShiftDown = false;

    private bool IsMuteRepeat = false;

    private LRESULT HookCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {

        var keyState = (KeyState)Enum.ToObject(typeof(KeyState), wParam);
        // 4:   Alt
        // 256: Down
        // 257: Up

        var key = (short)Marshal.ReadInt32(lParam);

        // フックしたキー
        //Debug.WriteLine($"code:{nCode} {keyState.ToString()}:({key})");

        if (wParam == 260) // Alt+Down
        {
            switch (key)
            {
                case 188: // '<'
                    SendKey((ushort)VIRTUAL_KEY.VK_VOLUME_DOWN);
                    break;
                case 190: // '<'
                    SendKey((ushort)VIRTUAL_KEY.VK_VOLUME_UP);
                    break;
                case 77: // 'M'
                    if (IsMuteRepeat)
                    {
                        break;
                    }

                    SendKey((ushort)VIRTUAL_KEY.VK_VOLUME_MUTE);
                    IsMuteRepeat = true; // MuteのRepeatを抑制する
                    break;
            }
        }
        else if (wParam == 261)  // Alt+Up
        {
            switch (key)
            {
                case 77: // 'M'
                    IsMuteRepeat = false;
                    break;
            }
        }

        // 1を戻すとフックしたキーが捨てられます
        return new LRESULT(0);
    }

    private void SendKey(ushort keycode)
    {
        Span<INPUT> inputs = stackalloc INPUT[2];

        inputs[0].type = INPUT_TYPE.INPUT_KEYBOARD;
        inputs[0].Anonymous.ki.wVk = (VIRTUAL_KEY)keycode;

        inputs[1].type = INPUT_TYPE.INPUT_KEYBOARD;
        inputs[1].Anonymous.ki.wVk = (VIRTUAL_KEY)keycode;
        inputs[1].Anonymous.ki.dwFlags = KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP;

        Windows.Win32.PInvoke.SendInput(inputs, Marshal.SizeOf<INPUT>());
    }


    public void Start()
    {
        if(IsStarted)
        {
            return;
        }

        StartHook();
    }

    public void Stop()
    {
        if (hookHandle == null)
        {
            return;
        }
        var ptr = hookHandle.DangerousGetHandle();
        
        Windows.Win32.PInvoke.UnhookWindowsHookEx(new HHOOK(ptr));

        hookHandle.Dispose();
        hookHandle = null;
    }
}
