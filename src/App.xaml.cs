using ClipboardZenHanConverter.Core.Logic;
using ClipboardZenHanConverter.Core.Models;
using ClipboardZenHanConverter.Core.Services;
using ClipboardZenHanConverter.Services;
using ClipboardZenHanConverter.ViewModels;
using ClipboardZenHanConverter.Views;
using ClipboardZenHanConverter.Views.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardZenHanConverter
{
    /// <summary>アプリケーションのエントリポイントとなるクラスです。</summary>
    /// <remarks>
    /// DIコンテナの初期化、サービス・ViewModel・Viewの登録、例外ハンドラの設定、画面遷移サービスの初期化など、アプリ全体のライフサイクル管理を行います。<br/>
    /// </remarks>
    public partial class App : Application
    {
        /// <summary>DIコンテナからサービスを取得するためのアプリケーション全体で利用可能なプロバイダーです。</summary>
        /// <remarks>
        /// 初期化前にアクセスすると InvalidOperationException がスローされます。<br/>
        /// </remarks>
        public static IServiceProvider Services
        {
            get => field ?? throw new InvalidOperationException("ServiceProvider is not initialized  within App.xaml.cs.");
            private set;
        }

        /// <summary>DIコンテナから指定した型のサービスを取得します。</summary>
        /// <typeparam name="T">取得するサービスの型。クラス型のみ指定可能です。</typeparam>
        /// <returns>登録済みのサービスインスタンス</returns>
        /// <exception cref="ArgumentException">指定した型 T のサービスが DI コンテナに登録されていない場合にスローされます。</exception>
        public static T GetService<T>() where T : class
        {
            // DIコンテナから指定した型のサービスを取得
            if (Services.GetService(typeof(T)) is not T service)
            {
                throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
            }
            return service;
        }

        /// <summary>アプリケーションのエントリポイントとなるコンストラクタです。</summary>
        /// <remarks>
        /// DIコンテナの初期化、サービス・ViewModel・Viewの登録、例外ハンドラの設定を行います。<br/>
        /// </remarks>
        public App()
        {
            // WinUI コンポーネントの初期化
            InitializeComponent();

            // 未処理例外のイベントハンドラを設定
            UnhandledException += App_UnhandledException;

            // ホストビルダーを作成
            var builder = Host.CreateApplicationBuilder();

            // NavigationService をインターフェースとして登録
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            
            // ClipboardService をインターフェースとして登録
            builder.Services.AddSingleton<IClipboardService, ClipboardService>();

            // Views and ViewModels をシングルトンとして登録
            builder.Services.AddSingleton<MainWindow>();
            builder.Services.AddSingleton<MainWindowViewModel>();
            builder.Services.AddSingleton<HomePage>();
            builder.Services.AddSingleton<HomeViewModel>();
            builder.Services.AddSingleton<SettingsPage>();
            builder.Services.AddSingleton<SettingsViewModel>(); 
            builder.Services.AddSingleton<SettingsModel>();
            builder.Services.AddSingleton<AppSetting>();
            builder.Services.AddSingleton<ConvertConfig>();
            builder.Services.AddSingleton<CharConverter>();

            // ホストをビルドし、サービスプロバイダーを設定
            var host = builder.Build();
            Services = host.Services;

            // 各種設定の初期化
            GetService<AppSetting>().Initialize();
            GetService<ConvertConfig>().Initialize();
        }


        /// <summary>アプリケーション全体の未処理例外を捕捉するイベントハンドラです。</summary>
        /// <remarks>
        /// このメソッド内で例外のロギングやユーザーへの通知など、適切な例外処理を実装してください。<br/>
        /// 詳細は Microsoft Docs を参照してください。<br/>
        /// </remarks>
        /// <param name="sender">例外発生元のオブジェクト</param>
        /// <param name="e">未処理例外イベントの引数情報</param>
        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[UnhandledException] {e.Exception?.Message}");
            System.Diagnostics.Debug.WriteLine($"[UnhandledException] StackTrace: {e.Exception?.StackTrace}");

            // アプリの終了を防ぐため、例外を処理済みとしてマーク
            e.Handled = true;
        }

        /// <summary>アプリケーションの起動時に呼び出されるメソッドです。</summary>
        /// <remarks>
        /// DIコンテナから INavigationService を取得し、初期化処理を行います。<br/>
        /// </remarks>
        /// <param name="args">起動イベントの引数情報</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // DIコンテナから NavigationService を取得
            var navigationService = App.GetService<INavigationService>();

            // NavigationService が具象型の場合、初期化を実行
            if (navigationService is NavigationService navService)
            {
                navService.Initialize();
            }

            // メインウィンドウをアクティブ化
            App.GetService<MainWindow>().Activate();
        }
    }
}
