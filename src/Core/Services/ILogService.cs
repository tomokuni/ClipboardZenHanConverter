using System;

namespace ClipboardZenHanConverter.Core.Services;

/// <summary>アプリケーションのログ出力を提供するサービスです。</summary>
public interface ILogService
{
    /// <summary>例外情報を記録します。</summary>
    /// <param name="ex">例外オブジェクト</param>
    /// <param name="source">例外発生元の識別名</param>
    void LogException(Exception? ex, string source);

    /// <summary>トレース情報を記録します。</summary>
    /// <param name="message">トレースメッセージ</param>
    void LogTrace(string message);
}
