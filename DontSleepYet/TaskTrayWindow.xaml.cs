using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DontSleepYet.Contracts.Services;
using H.NotifyIcon.Core;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DontSleepYet;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class TaskTrayWindow : Window
{
    public TaskTrayWindow()
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

        notifyIcon.DoubleClickCommand = ShowWindowCommand;

        /// 
        Closed += (s, e) =>
        {
            if (!App.HandleClosedEvents)
            {
                notifyIcon.Dispose();
            }
        };        
    }

    public ICommand ExitApplicationCommand { get; }
    public ICommand ShowHideWindowCommand { get; }
    public ICommand ShowWindowCommand   { get; }

    private void Exit()
    {
        var app = Application.Current as App;
        if (app == null)
        {
            return;
        }

        App.HandleClosedEvents = false;
        
        app.CloseWindow();
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
}
