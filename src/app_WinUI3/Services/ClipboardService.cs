using ClipboardZenHanConverter.Core.Interfaces;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.DataTransfer;

namespace ClipboardZenHanConverter.App.WinUI.Services;

/// <summary>システムクリップボードの読み書きと内容変更監視を提供します。</summary>
/// <remarks>クリップボードへのアクセスはバックグラウンド時に拒否される場合があります。<br/>
/// その場合の例外は握り潰し、null または無操作で応答します。<br/>
/// IClipboardService インターフェースを実装し、DIP による依存性注入に対応します。</remarks>
public partial class ClipboardService : IClipboardService
{
    /// <summary>クリップボード内容変更イベント。</summary>
    public event EventHandler<object>? ContentChanged;

    /// <summary>ClipboardService の新しいインスタンスを初期化します。システムクリップボードの変更監視を開始します。</summary>
    public ClipboardService()
    {
        Clipboard.ContentChanged += OnClipboardContentChanged;
    }

    /// <summary>ContentChanged イベントを発行します。テスト用に public 公開。</summary>
    public void RaiseContentChanged() => ContentChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>システムクリップボードの内容変更時に呼び出されます。</summary>
    private void OnClipboardContentChanged(object? sender, object e)
        => ContentChanged?.Invoke(this, e);

    /// <summary>リソースを解放します。Clipboard.ContentChanged の購読を解除します。</summary>
    public void Dispose()
    {
        Clipboard.ContentChanged -= OnClipboardContentChanged;
        GC.SuppressFinalize(this);
    }

    /// <summary>クリップボードからテキストを非同期に取得します。</summary>
    /// <returns>取得したテキスト。取得できない場合は null。</returns>
    /// <remarks>バックグラウンド時などアクセスが拒否された場合は null を返します。</remarks>
    public virtual async Task<string?> GetTextAsync()
    {
        try
        {
            var dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
                return await dataPackageView.GetTextAsync();
        }
        catch (UnauthorizedAccessException)
        {
            // バックグラウンド時などクリップボードアクセスが拒否されるケースは
            // 想定内の通常動作であり、グローバルハンドラに委ねる必要はない。
        }
        catch (COMException)
        {
            // WinRT クリップボード API の COM 相互運用例外。アプリ動作に影響させない。
        }
        return null;
    }

    /// <summary>指定されたテキストをクリップボードに設定します。</summary>
    /// <param name="text">設定するテキスト。</param>
    public virtual void SetText(string text) => ClipboardActionSafe(() =>
    {
        var data = new DataPackage();
        data.SetText(text);
        Clipboard.SetContent(data);
    });

    /// <summary>クリップボードの内容をフラッシュ（永続化）します。</summary>
    /// <remarks>アプリケーション終了後もクリップボードの内容を保持するために呼び出します。</remarks>
    public virtual void Flush() => ClipboardActionSafe(() => Clipboard.Flush());

    /// <summary>クリップボード操作を安全に実行します。アクセス拒否時は例外を無視します。</summary>
    /// <param name="action">実行するクリップボード操作。</param>
    /// <remarks>バックグラウンド状態の変更ではクリップボードAPIが例外をスローすることがありますが、アプリの動作に影響しないため無視します。</remarks>
    private static void ClipboardActionSafe(Action action)
    {
        try
        {
            action();
        }
        catch (UnauthorizedAccessException)
        {
            // バックグラウンド時などクリップボードアクセスが拒否されるケースは
            // 想定内の通常動作であり、グローバルハンドラに委ねる必要はない。
        }
        catch (COMException)
        {
            // WinRT クリップボード API の COM 相互運用例外。アプリ動作に影響させない。
        }
    }
}
