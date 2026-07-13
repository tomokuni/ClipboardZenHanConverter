using ClipboardZenHanConverter.Core.Interfaces;
using ClipboardZenHanConverter.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using System.Runtime.InteropServices;

namespace ClipboardZenHanConverter.App.ViewModels;

/// <summary>ホーム画面のデータを管理し、クリップボード監視と文字変換を実行します。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - クリップボードの内容変更監視と自動文字変換<br/>
/// - 変換結果のクリップボードへの書き戻し<br/>
/// - 変換中の再帰的イベント防止（_isUpdatingClipboard フラグ + SemaphoreSlim）<br/>
/// - 設定画面へのナビゲーション<br/><br/>
/// 特徴: <br/>
/// - IClipboardService / ITextConverter / INavigationService インターフェースに依存（DIP）<br/>
/// - テスト時は各インターフェースのモック実装で差し替え可能<br/>
/// - SemaphoreSlim による排他制御でクリップボード変更イベントの重複処理を防止<br/><br/>
/// 処理フロー: <br/>
/// 1. クリップボード変更イベント受信<br/>
/// 2. 排他制御（SemaphoreSlim.TryWaitAsync）<br/>
/// 3. BeforeText 更新<br/>
/// 4. ITextConverter.Convert で変換実行<br/>
/// 5. 変換結果が元のテキストと異なる場合のみクリップボードに書き戻し<br/>
/// 6. _isUpdatingClipboard フラグで書き戻し中の再帰イベントを防止</remarks>
public partial class HomeViewModel : ObservableObject, IDisposable
{
    /// <summary>テキスト変換器（DIP: ITextConverter）。</summary>
    private readonly ITextConverter _converter;
    /// <summary>クリップボードサービス（DIP: IClipboardService）。</summary>
    private readonly IClipboardService _clipboardService;
    /// <summary>ナビゲーションサービス（DIP: INavigationService）。</summary>
    private readonly INavigationService _navigation;
    /// <summary>UIスレッドへのディスパッチキュー。</summary>
    private readonly DispatcherQueue? _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    /// <summary>クリップボード変更イベントの排他制御用セマフォ。</summary>
    private readonly SemaphoreSlim _clipboardSemaphore = new(1, 1);
    /// <summary>クリップボード書き戻し中フラグ（再帰防止）。</summary>
    private bool _isUpdatingClipboard;
    /// <summary>Dispose 済みフラグ。</summary>
    private bool _disposed;

    /// <summary>テスト用: true に設定すると同期実行します。</summary>
    public bool TestMode { get; set; }

    /// <summary>変換前のテキストを取得または設定します。</summary>
    [ObservableProperty]
    public partial string BeforeText { get; set; } = "";

    /// <summary>変換後のテキストを取得または設定します。</summary>
    [ObservableProperty]
    public partial string ConvertedText { get; set; } = "";

    /// <summary>変換設定を取得します。</summary>
    public ConvertConfig Config { get; }

    /// <summary>設定画面へ遷移します。</summary>
    [RelayCommand]
    private void NavigateToSettings() => _navigation.NavigateTo("Settings");

    /// <summary>HomeViewModel の新しいインスタンスを初期化します。</summary>
    /// <param name="config">変換設定</param>
    /// <param name="converter">テキスト変換器</param>
    /// <param name="clipboardService">クリップボードサービス</param>
    /// <param name="navigation">ナビゲーションサービス</param>
    public HomeViewModel(ConvertConfig config, ITextConverter converter,
        IClipboardService clipboardService, INavigationService navigation)
    {
        Config = config;
        _converter = converter;
        _clipboardService = clipboardService;
        _navigation = navigation;
        _clipboardService.ContentChanged += Clipboard_ContentChanged;
    }

    /// <summary>リソースを解放します。クリップボード変更イベントの購読を解除し、セマフォを破棄します。</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _clipboardService.ContentChanged -= Clipboard_ContentChanged;
        _clipboardSemaphore.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>クリップボードの内容変更時に呼び出されます。UIスレッドまたはバックグラウンドで変換処理を開始します。</summary>
    private void Clipboard_ContentChanged(object? sender, object e)
    {
        if (TestMode)
        {
            _ = HandleClipboardChangeAsync();
            return;
        }

        if (_dispatcherQueue is not null)
        {
            _dispatcherQueue.TryEnqueue(async () => await HandleClipboardChangeAsync());
        }
        else
        {
            _ = Task.Run(async () => await HandleClipboardChangeAsync());
        }
    }

    /// <summary>クリップボード変更を非同期に処理します。</summary>
    /// <remarks>
    /// SemaphoreSlim で排他制御を行い、複数のイベントが同時に処理されるのを防ぎます。<br/>
    /// 処理フロー: <br/>
    /// 1. SemaphoreSlim で排他ロック取得（即時利用不可の場合はスキップ）<br/>
    /// 2. _isUpdatingClipboard フラグチェック（クリップボード書き戻し中のイベントを無視）<br/>
    /// 3. クリップボードからテキスト取得<br/>
    /// 4. 変換結果が空または元と同じならスキップ<br/>
    /// 5. 変換結果をクリップボードに書き戻し</remarks>
    private async Task HandleClipboardChangeAsync()
    {
        if (!await _clipboardSemaphore.WaitAsync(0))
            return;

        try
        {
            if (_isUpdatingClipboard)
                return;

            var text = await _clipboardService.GetTextAsync();
            if (string.IsNullOrEmpty(text))
                return;

            if (ConvertedText == text)
                return;

            BeforeText = text;
            ConvertedText = "";

            var convertedText = _converter.Convert(text);
            if (text == convertedText)
            {
                ConvertedText = "変換不要 (変更なし)";
                return;
            }

            ConvertedText = convertedText;

            try
            {
                _isUpdatingClipboard = true;
                _clipboardService.SetText(convertedText);
                _clipboardService.Flush();
            }
            finally
            {
                _isUpdatingClipboard = false;
            }
        }
        catch (UnauthorizedAccessException)
        {
            // バックグラウンド時のクリップボードアクセス拒否は想定内の通常動作。
        }
        catch (COMException)
        {
            // WinRT クリップボード API の COM 相互運用例外も想定内。
        }
        finally
        {
            _clipboardSemaphore.Release();
        }
    }
}
