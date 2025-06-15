using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontSleepYet.Helpers;

public static class StartUpHelper
{
    //
    // スタートアップにショートカットを登録する
    //
    public static void RegiserStartUp_CurrentUserRun(string title = null)
    {
        //var path = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
        //var filename = Path.GetFileName(Application.ExecutablePath);
        //var app = System.Windows.Application.Current;
        //var executablePath = Environment.GetCommandLineArgs()[0];
        var executablePath = Environment.ProcessPath!;
        

        if (title == null)
        {
            title = Path.GetFileNameWithoutExtension(executablePath);
        }

        var path = GetStartupShortcutPath(title);

        makeShortcut(executablePath, path);
    }

    //
    // スタートアップからショートカットを削除する
    //
    public static void UnregiserStartUp_CurrentUserRun(string title = null)
    {
        if (title == null)
        {
            var ExecutablePath = Environment.GetCommandLineArgs()[0];
            title = Path.GetFileNameWithoutExtension(ExecutablePath);
        }

        var path = GetStartupShortcutPath(title);
        File.Delete(path);
    }

    public static bool ExistsStartUp_CurrentUserRun(string title = null)
    {
        if (title == null)
        {
            var ExecutablePath = Environment.GetCommandLineArgs()[0];
            title = Path.GetFileNameWithoutExtension(ExecutablePath);
        }

        var path = GetStartupShortcutPath(title);
        return File.Exists(path);
    }

    static string GetStartupShortcutPath(string title)
    {
        return System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Startup), @$"{title}.lnk");
    }

    public static void makeShortcut(string path, string target)
    {
        //作成するショートカットのパス
        //string shortcutPath = System.IO.Path.Combine(
        //    Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory),
        //    @"MyApp.lnk");
        //ショートカットのリンク先
        //string targetPath = Application.ExecutablePath;

        //WshShellを作成
        var t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"));
        dynamic shell = Activator.CreateInstance(t);
        //WshShortcutを作成
        var shortcut = shell.CreateShortcut(target);

        //リンク先
        shortcut.TargetPath = path;
        //アイコンのパス
        shortcut.IconLocation = path + ",0";
        //その他のプロパティも同様に設定できるため、省略

        //ショートカットを作成
        shortcut.Save();

        //後始末
        System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
        System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
    }
}
