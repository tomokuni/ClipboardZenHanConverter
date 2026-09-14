using Aprillz.MewUI;
using EsUtil.ClipboardZenHanConverter.App.MewUI.Services;
using EsUtil.ClipboardZenHanConverter.App.MewUI.Views;
using EsUtil.ClipboardZenHanConverter.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EsUtil.ClipboardZenHanConverter.App.MewUI;

/// <summary>アプリケーションのエントリポイントを提供します。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - DIコンテナ（Microsoft.Extensions.Hosting）の初期化とサービス登録<br/>
/// - 集約エラーハンドラー（UIスレッド、バックグラウンドスレッド、非同期タスク）<br/>
/// - 起動時のメインウィンドウ表示<br/><br/>
/// 特徴: <br/>
/// - サービス登録は DependencyInjectionExtensions に分離（単一責任の原則）<br/>
/// - MewUI の ApplicationBuilder によるウィンドウ構築
/// </remarks>
public static class Program
{
    /// <summary>アプリケーションのエントリポイントです。</summary>
    public static void Main()
    {
        SubscribeExceptionHandlers();

        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddClipboardZenHanConverterServices();
        var host = builder.Build();

        // 設定を読み込み、自動保存を開始
        host.Services.GetRequiredService<AppSetting>().Initialize();
        host.Services.GetRequiredService<ConvertConfig>().Initialize();

        Application
            .Create()
            .UseWin32()
            .UseDirect2D()
            .BuildMainWindow(() => host.Services.GetRequiredService<MainWindow>())
            .Run();
    }

    /// <summary>集約エラーハンドラーを購読します。UI/バックグラウンド/非同期タスクの未処理例外を補足します。</summary>
    private static void SubscribeExceptionHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                Log($"[致命的]: {ex.Message}");
        };
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log($"[Task]: {e.Exception.Message}");
            e.SetObserved();
        };
        Application.DispatcherUnhandledException += e =>
        {
            Log($"[UI]: {e.Exception.Message}");
            e.Handled = true;
        };
    }

    /// <summary>ログを出力します。</summary>
    /// <param name="message">出力するメッセージ。</param>
    private static void Log(string message) => Debug.WriteLine(message);
}
