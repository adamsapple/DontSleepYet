using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Common;
using DontSleepYet.Activation;
using DontSleepYet.Contracts.Services;
using DontSleepYet.Core.Contracts.Services;
using DontSleepYet.Core.Services;
using DontSleepYet.Helpers;
using DontSleepYet.Models;
using DontSleepYet.Notifications;
using DontSleepYet.Services;
using DontSleepYet.ViewModels;
using DontSleepYet.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Composition.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Windows.Graphics;

namespace DontSleepYet;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();
    // public static System.Drawing.Icon Icon { get; private set; }

    public static bool HandleClosedEvents { get; set; } = true;

    public static UIElement? AppTitlebar { get; set; }

    private readonly ILocalSettingsService localSettingsService;

    public App()
    {
        InitializeComponent();


        // 2重起動抑止
        {
            var assembly = Assembly.GetExecutingAssembly();
            var mutexName = string.Format("{0}-{1}-{2}",
                              Process.GetCurrentProcess().ProcessName,
                              assembly.GetCustomAttribute<AssemblyProductAttribute>()!.Product,
                              assembly.GetCustomAttribute<AssemblyCompanyAttribute>()!.Company);

            var mainInstance = AppInstance.FindOrRegisterForKey(mutexName);

            if (!mainInstance.IsCurrent)
            {
                // 既に起動している場合は、こちらをKiilして終了
                Process.GetCurrentProcess().Kill();
                return;
            }
        }

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers
            services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

            // Services
            services.AddSingleton<IAppNotificationService, AppNotificationService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();
            services.AddSingleton<IDontSleepService, DontSleepService>();
            services.AddSingleton<IKeyHookService, KeyHookService>();
            services.AddSingleton<ISystemInfoLiteService, SystemInfoLiteService>();
            services.AddSingleton<IUpdateCheckService, GithubUpdateCheckService>();
            services.AddSingleton<IUpdateNotificationService, UpdateNotificationService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            if (RuntimeHelper.IsMSIX)
            {
                services.AddSingleton<IUpdateService, PackagedUpdateService>();
            }
            else
            {
                services.AddSingleton<IUpdateService, GithubUnpackagedUpdateService>();
            }

            // Core Services
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddSingleton<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddSingleton<MainViewModel>();
            services.AddTransient<MainPage>();
            services.AddTransient<ShellPage>();
            services.AddSingleton<ShellViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
            services.Configure<GithubBaseOption>(context.Configuration.GetSection("Github"));
        }).
        Build();

        App.GetService<IAppNotificationService>().Initialize();
        localSettingsService = App.GetService<ILocalSettingsService>();

        App.GetService<IKeyHookService>().Start();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        /// show Toast notification.
        //App.GetService<IAppNotificationService>().Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));

        await MainWindowSetting();

        await App.GetService<IActivationService>().ActivateAsync(args);

#if !DEBUG
        MainWindow.Hide();
#endif

        /// 更新確認サービスを開始
        await App.GetService<IUpdateNotificationService>().StartAsync();
    }

    private bool WindowPositionSettingCalled = false;

    /// <summary>
    /// - MainWindowの位置を復元する
    /// - 終了時にMainWindowの位置を保存する
    /// - Windowのクローズボタン押下時は、MainWindowを隠すだけにする設定を付与
    /// </summary>
    /// <returns></returns>
    private async Task MainWindowSetting()
    {
        if (WindowPositionSettingCalled)
        {
            return;
        }

        /// MainWindowの位置を復元する
        {
            var pos = await localSettingsService.ReadSettingAsync<PointInt32>("WindowPosition.Pos");
            MainWindow.AppWindow.Move(pos);
        }

        MainWindow.Closed += (sender, args) =>
        {
            /// Save the position of the MainWindow when it is closed
            {
                var pos = MainWindow.AppWindow.Position;
                //Debug.WriteLine($"MainWindow closed. Position: {pos.X}, {pos.Y}");

                //var localSettingsService = App.GetService<ILocalSettingsService>();
                localSettingsService.SaveSettingAsync("WindowPosition.Pos", pos).GetResultOrDefault();
            }

            /// Windowのクローズボタン押下時は、MainWindowを隠すだけにする
            if (HandleClosedEvents)
            {
                args.Handled = true;
                MainWindow.Hide();
            }
        };

        MainWindow.SizeChanged += (sender, args) => { 
        };

        WindowPositionSettingCalled = true;
    }

    public void CloseWindow()
    {
        MainWindow.Close();
    }
}
