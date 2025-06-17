using System.Collections.Specialized;
using System.Diagnostics;
using System.Web;

using DontSleepYet.Contracts.Services;
using DontSleepYet.ViewModels;

using Microsoft.Windows.AppNotifications;
using static CommunityToolkit.WinUI.Animations.Expressions.ExpressionValues;

namespace DontSleepYet.Notifications;

public class AppNotificationService : IAppNotificationService
{
    private readonly INavigationService _navigationService;

    public AppNotificationService(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    ~AppNotificationService()
    {
        Unregister();
    }

    public void Initialize()
    {
        AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;

        AppNotificationManager.Default.Register();
    }

    public void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        // TODO: Handle notification invocations when your app is already running.

        //// // Navigate to a specific page based on the notification arguments.
        //// if (ParseArguments(args.Argument)["action"] == "Settings")
        //// {
        ////    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        ////    {
        ////        _navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
        ////    });
        //// }
        
        
        //var action = ParseArguments(args.Argument)["action"] ?? string.Empty;

        var action = args.Arguments["action"] ?? string.Empty;

        if (action == "VisitInfo")
        {
            var url = args.Arguments["url"] ?? string.Empty;
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                ProcessStartInfo pi = new ProcessStartInfo()
                {
                    FileName = url,
                    UseShellExecute = true,
                };

                System.Diagnostics.Process.Start(pi);
            });
        }else if (action == "Ignore")
        {
            var updateNotificationService = App.GetService<IUpdateNotificationService>();
            updateNotificationService.IgnoreThisVersion();
        }

        //App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        //{
        //    App.MainWindow.ShowMessageDialogAsync("TODO: Handle notification invocations when your app is already running.", "Notification Invoked");

        //    App.MainWindow.BringToFront();
        //});
    }

    public bool Show(string payload)
    {
        var appNotification = new AppNotification(payload);

        AppNotificationManager.Default.Show(appNotification);

        return appNotification.Id != 0;
    }

    public NameValueCollection ParseArguments(string arguments)
    {
        return HttpUtility.ParseQueryString(arguments);
        //return ToastArguments.Parse(arguments);
    }

    public void Unregister()
    {
        AppNotificationManager.Default.Unregister();
    }
}
