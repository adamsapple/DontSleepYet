using System.Diagnostics;
using System.Threading;
using DontSleepYet.Contracts.Services;
using DontSleepYet.Helpers;
using DontSleepYet.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics;
using Windows.UI.ViewManagement;

namespace DontSleepYet;

public sealed partial class MainWindow : WindowEx
{
    #region Services.
    private ILocalSettingsService localSettingsService;
    #endregion Services.

    private Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;

    private UISettings settings;


    public MainWindow()
    {
        this.localSettingsService = App.GetService<ILocalSettingsService>();
        InitializeComponent();

        //AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        
        Content = null;
        Title = "AppDisplayName".GetLocalized();
        // AppWindow.SetIcon("ms-appx:///Assets/WindowIcon.ico");

        // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        settings = new UISettings();
        settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event        
    }

    // this handles updating the caption button colors correctly when indows system theme is changed
    // while the app is open
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        dispatcherQueue.TryEnqueue(() =>
        {
            TitleBarHelper.ApplySystemThemeToCaptionButtons();
        });
    }

    //private bool restoredWindowPosition = false;
    //private async void WindowEx_Activated(object sender, WindowActivatedEventArgs args)
    //{
    //    if(args.WindowActivationState == WindowActivationState.CodeActivated && !restoredWindowPosition)
    //    {
    //        //PointInt32 pos = new PointInt32
    //        //{
    //        //    X = await localSettingsService.ReadSettingAsync<int>("WindowPosition.X"),
    //        //    Y = await localSettingsService.ReadSettingAsync<int>("WindowPosition.Y")
    //        //};

    //        var pos = await localSettingsService.ReadSettingAsync<PointInt32>("WindowPosition.Pos");

    //        //AppWindow.Move(pos);

    //        restoredWindowPosition = true;
    //    }
    //}

    //private void WindowEx_Closed(object sender, WindowEventArgs args)
    //{
    //    args.Handled = true;
    //    this.Hide();
    //}
}
