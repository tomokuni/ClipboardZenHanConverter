using EsUtil.ClipboardZenHanConverter.App.WinForms.Services;
using EsUtil.ClipboardZenHanConverter.App.WinForms.Views;
using EsUtil.ClipboardZenHanConverter.Core.Models;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EsUtil.ClipboardZenHanConverter.App.WinForms;

/// <summary>アプリケーションのエントリポイントを提供します。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - WinForms アプリケーションの構成と起動<br/>
/// - DI コンテナ（Microsoft.Extensions.Hosting）の初期化とサービス登録<br/>
/// - 集約エラーハンドラー（UI スレッド、バックグラウンドスレッド、非同期タスク）<br/><br/>
/// 特徴: <br/>
/// - サービス登録は <see cref="DependencyInjectionExtensions"/> に分離（単一責任の原則）<br/>
/// - <see cref="ApplicationConfiguration.Initialize"/> が csproj の設定に従って
///   DPI 認識・ビジュアルスタイル・テキストレンダリング方式を初期化する
/// </remarks>
public static class Program
{
    /// <summary>DI コンテナのサービスプロバイダーを取得します。</summary>
    /// <value>ビルド済みの IServiceProvider インスタンス。</value>
    private static IServiceProvider Services { get; set; } = null!;

    /// <summary>アプリケーションのエントリポイントです。</summary>
    [STAThread]
    public static void Main()
    {
        ApplicationConfiguration.Initialize();
        SubscribeExceptionHandlers();

        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddClipboardZenHanConverterServices();
        var host = builder.Build();

        Services = host.Services;
        GetService<AppSetting>().Initialize();
        GetService<ConvertConfig>().Initialize();

        Application.Run(GetService<MainForm>());
    }

    /// <summary>DI コンテナから指定した型のサービスを取得します。</summary>
    /// <typeparam name="T">取得するサービスの型。</typeparam>
    /// <returns>登録されたサービスインスタンス。</returns>
    /// <exception cref="InvalidOperationException">指定された型が DI コンテナに登録されていない場合。</exception>
    private static T GetService<T>() where T : class
        => Services.GetService(typeof(T)) as T
            ?? throw new InvalidOperationException($"{typeof(T)} is not registered in DI container.");

    /// <summary>集約エラーハンドラーを購読します。UI/バックグラウンド/非同期タスクの未処理例外を補足します。</summary>
    private static void SubscribeExceptionHandlers()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        Application.ThreadException += (_, e) => Log($"[UI]: {e.Exception.Message}");
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
