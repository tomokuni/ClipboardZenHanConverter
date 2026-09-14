using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ClipboardZenHanConverter.App.AvaloniaUI.Services;
using ClipboardZenHanConverter.App.AvaloniaUI.Views;
using ClipboardZenHanConverter.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace ClipboardZenHanConverter.App.AvaloniaUI;

/// <summary>アプリケーションのエントリポイントとなるクラスです。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - DIコンテナ（Microsoft.Extensions.Hosting）の初期化とサービス登録<br/>
/// - 集約エラーハンドラー（UIスレッド、バックグラウンドスレッド、非同期タスク）<br/>
/// - 起動時のメインウィンドウ表示<br/><br/>
/// 特徴: <br/>
/// - サービス登録は DependencyInjectionExtensions に分離（単一責任の原則）<br/>
/// - static な <see cref="Services"/> プロパティによる DI コンテナへの簡易アクセス<br/>
/// - field キーワード（C# 14）を使用した簡潔なプロパティ実装
/// </remarks>
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

    /// <summary>XAML を読み込みます。</summary>
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    /// <summary>フレームワークの初期化完了時に呼び出されます。DI コンテナを構築し、メインウィンドウを表示します。</summary>
    /// <remarks>処理フロー: <br/>
    /// 1. 集約エラーハンドラーを購読<br/>
    /// 2. Host を構築してサービスを登録<br/>
    /// 3. 設定（AppSetting / ConvertConfig）を読み込み、自動保存を開始<br/>
    /// 4. デスクトップライフタイムへメインウィンドウを設定</remarks>
    public override void OnFrameworkInitializationCompleted()
    {
        SubscribeExceptionHandlers();

        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddClipboardZenHanConverterServices();
        var host = builder.Build();

        Services = host.Services;
        GetService<AppSetting>().Initialize();
        GetService<ConvertConfig>().Initialize();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = GetService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>集約エラーハンドラーを購読します。UI/バックグラウンド/非同期タスクの未処理例外を補足します。</summary>
    private void SubscribeExceptionHandlers()
    {
        Dispatcher.UIThread.UnhandledException += (_, e) =>
        {
            Log($"[UI]: {e.Exception.Message}");
            e.Handled = true;
        };
    }

    /// <summary>ログを出力します。</summary>
    /// <param name="message">出力するメッセージ。</param>
    private static void Log(string message) => Debug.WriteLine(message);
}
