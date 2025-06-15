using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Windows.UI.ViewManagement;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Windows.Input;

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

        ExitApplicationCommand = new RelayCommand(Exit);
        ShowHideWindowCommand = new RelayCommand(ShowHideWindow);
        ShowWindowCommand = new RelayCommand(ShowWindow);

        notifyIcon.DoubleClickCommand = ShowWindowCommand;
    }

    public ICommand ExitApplicationCommand { get; }
    public ICommand ShowHideWindowCommand { get; }
    public ICommand ShowWindowCommand   { get; }

    private void Exit()
    {
        // Exit the application
        Application.Current.Exit();
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
    }

}
