using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DontSleepYet.Contracts.Services;
using DontSleepYet.Models;
using DontSleepYet.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DontSleepYet.Views;

public sealed partial class TrayIconView : UserControl
{
    public ICommand ExitApplicationCommand { get; }
    public ICommand ShowHideWindowCommand { get; }
    public ICommand ShowWindowCommand { get; }
    public ICommand UpdateCheckCommand { get; }
    public ICommand LaunchTetheringConfigCommand { get; }
    public ICommand ToggleDontSleepActiveCommand { get; }

    private LocalSettingsOptions LocalSettingsOptions { get; }


    public TrayIconView()   // LocalSettingsOptions localSettingsOptions
    {
        InitializeComponent();

        {
            var iconStream = typeof(App).Assembly.GetManifestResourceStream("DontSleepYet.Assets.WindowIcon.ico");
            if (iconStream == null)
            {
                throw new InvalidOperationException("Resource stream for 'WindowIcon.ico' could not be found.");
            }

            notifyIcon.Icon = new System.Drawing.Icon(iconStream);
        }

        ExitApplicationCommand = new RelayCommand(Exit);
        ShowHideWindowCommand = new RelayCommand(ShowHideWindow);
        ShowWindowCommand = new RelayCommand(ShowWindow);
        UpdateCheckCommand = new RelayCommand(UpdateCheckAsync);
        LaunchTetheringConfigCommand = new RelayCommand(LaunchTetheringConfig);
        ToggleDontSleepActiveCommand = new RelayCommand(ToggleDontSleepActive);

        //LocalSettingsOptions = localSettingsOptions;
        //LocalSettingsOptions = App.GetService<LocalSettingsOptions>();ttuu

        notifyIcon.DoubleClickCommand = ShowWindowCommand;
        LocalSettingsOptions = App.GetService<LocalSettingsOptions>();
    }


    private void Exit()
    {
        var app = Application.Current as App;
        if (app == null)
        {
            return;
        }

        App.HandleClosedEvents = false;
        
        app.CloseWindow();
        notifyIcon.Dispose();
        app.Exit();
    }

    private void ShowHideWindow()
    {
        var window = App.MainWindow;
        if (window == null)
        {
            return;
        }

        if (window.Visible)
        {
            window.Hide();
        }
        else
        {
            window.Show();
            window.SetForegroundWindow();
            window.Activate();
        }
    }

    private void ShowWindow()
    {
        var window = App.MainWindow;
        if (window == null)
        {
            return;
        }

        if (!window.Visible)
        {
            window.Show();
        }

        window.SetForegroundWindow();
        window.Activate();
    }

    private async void UpdateCheckAsync()
    {
        var updateNotificationService = App.GetService<IUpdateNotificationService>()!;
        await updateNotificationService.StopAsync();
        await updateNotificationService.CheckAndNotificationAsync();
        await updateNotificationService.StartAsync();
    }

    private void LaunchTetheringConfig()
    {
        var controller = new Mobilespot.MobileHotspotController();
        controller.LaunchConfig();
    }

    private void ToggleDontSleepActive()
    {
        LocalSettingsOptions.IsDontSleepActive = !LocalSettingsOptions.IsDontSleepActive;
    }
}
