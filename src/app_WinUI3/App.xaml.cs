using EsUtil.ClipboardZenHanConverter.App.WinUI.Services;
using EsUtil.ClipboardZenHanConverter.App.WinUI.Views;
using EsUtil.ClipboardZenHanConverter.Core.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EsUtil.ClipboardZenHanConverter.App.WinUI;

/// <summary>アプリケーションのエントリポイントとなるクラスです。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - DIコンテナ（Microsoft.Extensions.Hosting）の初期化とサービス登録<br/>
/// - 集約エラーハンドラー（UIスレッド、バックグラウンドスレッド、非同期タスク）<br/>
/// - 起動時のメインウィンドウ表示と初期ページ設定<br/><br/>
/// 特徴: <br/>
/// - サービス登録は DependencyInjectionExtensions に分離（単一責任の原則）<br/>
/// - static な Services プロパティによる DI コンテナへの簡易アクセス<br/>
/// - field キーワード（C# 14）を使用した簡潔なプロパティ実装</remarks>
public partial class App : Application
{
    /// <summary>DIコンテナのサービスプロバイダーを取得します。初期化前にアクセスすると例外をスローします。</summary>
    /// <value>ビルド済みの IServiceProvider インスタンス。</value>
    /// <exception cref="InvalidOperationException">ServiceProvider が初期化されていない場合。</exception>
    public static IServiceProvider Services
    {
        get => field ?? throw new InvalidOperationException("ServiceProvider is not initialized.");
        private set;
    }

    /// <summary>DIコンテナから指定した型のサービスを取得します。</summary>
    /// <typeparam name="T">取得するサービスの型。</typeparam>
    /// <returns>登録されたサービスインスタンス。</returns>
    /// <exception cref="InvalidOperationException">指定された型が DI コンテナに登録されていない場合。</exception>
    public static T GetService<T>() where T : class
        => Services.GetService(typeof(T)) as T
            ?? throw new InvalidOperationException($"{typeof(T)} is not registered in DI container.");

    /// <summary>App の新しいインスタンスを初期化します。集約エラーハンドラーと DI コンテナを設定します。</summary>
    public App()
    {
        InitializeComponent();

        SubscribeExceptionHandlers();

        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddClipboardZenHanConverterServices();
        var host = builder.Build();

        Services = host.Services;
        GetService<AppSetting>().Initialize();
        GetService<ConvertConfig>().Initialize();
    }

    /// <summary>集約エラーハンドラーを購読します。UI/バックグラウンド/非同期タスクの未処理例外を補足します。</summary>
    private void SubscribeExceptionHandlers()
    {
        UnhandledException += OnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    /// <summary>UIスレッドの未処理例外を処理します。ログ出力後、Handled = true でアプリのクラッシュを防止します。</summary>
    private static void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Debug.WriteLine($"[集約ログ(UI)]: {e.Exception.Message}");
        e.Handled = true;
    }

    /// <summary>バックグラウンドスレッドの致命的な未処理例外を処理します。ログ出力のみ行います。</summary>
    private static void OnCurrentDomainUnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            Debug.WriteLine($"[集約ログ(致命的)]: {ex.Message}");
    }

    /// <summary>待機されなかったタスクの未処理例外を処理します。例外を観測済みとしてマークし、アプリのクラッシュを防止します。</summary>
    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Debug.WriteLine($"[集約ログ(Task)]: {e.Exception.Message}");
        e.SetObserved();
    }

    /// <summary>アプリケーションが起動された時に呼び出されます。メインウィンドウを表示し、初期ページを設定します。</summary>
    /// <param name="args">起動引数。</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var mainWindow = GetService<MainWindow>();
        var homeItem = mainWindow.NavigationView.MenuItems
            .OfType<Microsoft.UI.Xaml.Controls.NavigationViewItem>().FirstOrDefault();
        if (homeItem is not null) mainWindow.ViewModel.SelectedPage = homeItem;

        mainWindow.Activate();

        var navigation = GetService<INavigationService>();
        navigation.Initialize();

        // SettingsPage をバックグラウンドで事前生成: UIスレッドがアイドルになったタイミングで
        // XAML解析とページ生成を実行し、初回設定画面遷移を高速化する
        navigation.PreloadSettingsAsync();
    }
}
