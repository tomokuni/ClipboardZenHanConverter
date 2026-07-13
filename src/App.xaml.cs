using ClipboardZenHanConverter.Core.Logic;
using ClipboardZenHanConverter.Core.Models;
using ClipboardZenHanConverter.Core.Services;
using ClipboardZenHanConverter.Services;
using ClipboardZenHanConverter.ViewModels;
using ClipboardZenHanConverter.Views;
using ClipboardZenHanConverter.Core.Interfaces;
using ClipboardZenHanConverter.Views.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ClipboardZenHanConverter;

/// <summary>アプリケーションのエントリポイントとなるクラスです。</summary>
public partial class App : Application
{
    /// <summary>DIコンテナのサービスプロバイダーです。</summary>
    public static IServiceProvider Services
    {
        get => field ?? throw new InvalidOperationException("ServiceProvider is not initialized.");
        private set;
    }

    /// <summary>DIコンテナから指定した型のサービスを取得します。</summary>
    public static T GetService<T>() where T : class
    {
        if (Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in DI container.");
        }
        return service;
    }

    public App()
    {
        InitializeComponent();

        UnhandledException += App_UnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        var builder = Host.CreateApplicationBuilder();

        // サービス登録
        RegisterServices(builder.Services);

        var host = builder.Build();
        Services = host.Services;

        // 各種設定の初期化
        GetService<AppSetting>().Initialize();
        GetService<ConvertConfig>().Initialize();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        // ログサービス
        services.AddSingleton<ILogService, LogService>();

        // ナビゲーション
        services.AddSingleton<INavigationService, NavigationService>();

        // クリップボード
        services.AddSingleton<IClipboardService, ClipboardService>();

        // コアロジック
        services.AddSingleton<CharConverter>();

        // モデル
        services.AddSingleton<AppSetting>();
        services.AddSingleton<ConvertConfig>();
        services.AddSingleton<SettingsModel>();

        // ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<SettingsViewModel>();

        // Views
        services.AddSingleton<MainWindow>();
        services.AddSingleton<HomePage>();
        services.AddSingleton<SettingsPage>();
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        GetService<ILogService>().LogException(e.Exception, "App_UnhandledException");
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            GetService<ILogService>().LogException(ex, "CurrentDomain_UnhandledException");
        }
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        GetService<ILogService>().LogException(e.Exception, "TaskScheduler_UnobservedTaskException");
        e.SetObserved();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        var mainWindow = GetService<MainWindow>();

        // ViewModel の SelectedPage を初期選択項目（Home）で設定
        var navigationView = mainWindow.NavigationView;
        var homeItem = navigationView.MenuItems.OfType<Microsoft.UI.Xaml.Controls.NavigationViewItem>().FirstOrDefault();
        if (homeItem is not null)
        {
            mainWindow.ViewModel.SelectedPage = homeItem;
        }

        var navigationService = GetService<INavigationService>();
        if (navigationService is NavigationService navService)
        {
            navService.Initialize();
        }

        mainWindow.Activate();
    }
}
