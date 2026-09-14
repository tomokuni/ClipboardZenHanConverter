using Avalonia;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EsUtil.ClipboardZenHanConverter.App.AvaloniaUI;

/// <summary>アプリケーションのエントリポイントを提供します。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - Avalonia アプリケーションの構成と起動<br/>
/// - 集約エラーハンドラー（バックグラウンドスレッド、非同期タスク）の購読<br/><br/>
/// 特徴: <br/>
/// - Avalonia の初期化前に Avalonia 依存の API を触らないようにするため、UI スレッドの例外は
///   <see cref="App"/> 側で購読します
/// </remarks>
public static class Program
{
    /// <summary>アプリケーションのエントリポイントです。</summary>
    /// <param name="args">起動引数。</param>
    [STAThread]
    public static void Main(string[] args)
    {
        SubscribeExceptionHandlers();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    /// <summary>Avalonia アプリケーションを構成します。ビジュアルデザイナからも参照されます。</summary>
    /// <returns>構成済みの <see cref="AppBuilder"/>。</returns>
    /// <remarks>開発者ツールは Debug 構成でのみ有効にします（Release へは含めません）。</remarks>
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();

    /// <summary>集約エラーハンドラーを購読します。バックグラウンド/非同期タスクの未処理例外を補足します。</summary>
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
    }

    /// <summary>ログを出力します。</summary>
    /// <param name="message">出力するメッセージ。</param>
    private static void Log(string message) => Debug.WriteLine(message);
}
