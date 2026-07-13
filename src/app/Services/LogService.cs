using System;
using System.Diagnostics;
using System.IO;
using ClipboardZenHanConverter.Core.Interfaces;

namespace ClipboardZenHanConverter.Services;

/// <summary>アプリケーションのログ出力を提供するサービスの実装です。</summary>
/// <remarks>
/// ログ出力先は以下の2系統です。<br/>
/// - %LOCALAPPDATA%\ClipboardZenHanConverter\error.log (例外)<br/>
/// - %LOCALAPPDATA%\ClipboardZenHanConverter\trace.log (トレース)<br/>
/// - System.Diagnostics.Debug.WriteLine (デバッガー出力)<br/>
/// </remarks>
public sealed class LogService : ILogService
{
    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ClipboardZenHanConverter");

    /// <summary>例外情報をログファイルとデバッガー出力に記録します。</summary>
    public void LogException(Exception? ex, string source)
    {
        var message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {source}\n{ex}\n\n";
        Debug.Write(message);

        try
        {
            Directory.CreateDirectory(LogDirectory);
            File.AppendAllText(Path.Combine(LogDirectory, "error.log"), message);
        }
        catch
        {
            // ログ記録に失敗してもアプリには影響させない
        }
    }

    /// <summary>トレース情報をログファイルとデバッガー出力に記録します。</summary>
    public void LogTrace(string message)
    {
        var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}\n";
        Debug.Write(logMessage);

        try
        {
            Directory.CreateDirectory(LogDirectory);
            File.AppendAllText(Path.Combine(LogDirectory, "trace.log"), logMessage);
        }
        catch
        {
            // ログ記録に失敗してもアプリには影響させない
        }
    }
}
