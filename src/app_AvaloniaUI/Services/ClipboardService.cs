using Avalonia.Threading;
using ClipboardZenHanConverter.Core.Interfaces;
using ClipboardZenHanConverter.Core.Logic;
using ClipboardZenHanConverter.Core.Native;
using System.Runtime.InteropServices;

namespace ClipboardZenHanConverter.App.AvaloniaUI.Services;

/// <summary>システムクリップボードの読み書きと内容変更監視を提供します。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - クリップボードのテキスト取得・設定<br/>
/// - クリップボードの内容変更監視（Win32 の <c>GetClipboardSequenceNumber</c> ポーリング）<br/>
/// - 書き戻し後のフラッシュ（アプリ終了後も内容を保持）<br/><br/>
/// 特徴: <br/>
/// - IClipboardService インターフェース実装（DIP）<br/>
/// - 読み書きは Core.Native の Win32Clipboard、変更検出は Core.Logic の ClipboardChangeDetector へ委譲<br/>
/// - DispatcherTimer によるポーリングでクリップボード変更を検出<br/>
/// - アクセス拒否時などは握り潰してアプリ動作を継続<br/><br/>
/// 注意点: <br/>
/// - ポーリングは DispatcherTimer で行い、ContentChanged を UI スレッドで発行します。<br/>
///   購読側（HomeViewModel）がバインド済みのプロパティを更新するため、UI スレッドでの発行が必要です
/// </remarks>
public sealed class ClipboardService : IClipboardService
{
    /// <summary>ポーリング間隔（ミリ秒）。Core の変更検出器が推奨値を単一所有します。</summary>
    private const int PollIntervalMs = ClipboardChangeDetector.RecommendedPollIntervalMs;

    /// <summary>クリップボード内容変更イベント。</summary>
    public event EventHandler<object>? ContentChanged;

    /// <summary>クリップボード内容の変更検出器。</summary>
    private readonly ClipboardChangeDetector _changeDetector = new(SafeGetSequence);

    /// <summary>クリップボード変更監視のタイマー。</summary>
    private readonly DispatcherTimer _timer = new()
    {
        Interval = TimeSpan.FromMilliseconds(PollIntervalMs),
    };

    /// <summary>Dispose 済みのフラグ。</summary>
    private bool _disposed;

    /// <summary>ClipboardService の新しいインスタンスを初期化し、クリップボード監視を開始します。</summary>
    public ClipboardService()
    {
        _timer.Tick += (_, _) => OnTimerTick();
        _timer.Start();
    }

    /// <summary>クリップボード変更を検出した場合に ContentChanged イベントを発行します。</summary>
    private void OnTimerTick()
    {
        if (_changeDetector.HasChanged())
            ContentChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>クリップボードシーケンス番号を安全に取得します。失敗時は 0 を返します。</summary>
    private static uint SafeGetSequence()
    {
        try
        {
            return Win32Clipboard.GetClipboardSequenceNumber();
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or ExternalException)
        {
            return 0;
        }
    }

    /// <summary>テスト用に ContentChanged イベントを発行します。</summary>
    public void RaiseContentChanged() => ContentChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>クリップボードからテキストを非同期に取得します。</summary>
    /// <returns>取得したテキスト。取得できない場合は null。</returns>
    public Task<string?> GetTextAsync()
    {
        try
        {
            return Task.FromResult(Win32Clipboard.GetText());
        }
        catch (ExternalException)
        {
            return Task.FromResult<string?>(null);
        }
    }

    /// <summary>指定されたテキストをクリップボードに設定します。</summary>
    /// <param name="text">設定するテキスト。</param>
    public void SetText(string text)
    {
        try
        {
            Win32Clipboard.SetText(text);
        }
        catch (ExternalException)
        {
            // バックグラウンド時などクリップボードアクセスが拒否されるケースは想定内の通常動作。
        }
    }

    /// <summary>クリップボードの内容をフラッシュ（永続化）します。</summary>
    /// <remarks>CF_UNICODETEXT は SetClipboardData 時点で永続化されるため、明示的なフラッシュ操作は不要です。</remarks>
    public void Flush()
    {
    }

    /// <summary>リソースを解放します。タイマーを停止します。</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _timer.Stop();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
