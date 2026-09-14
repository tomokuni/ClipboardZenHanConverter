using Avalonia.Threading;
using EsUtil.ClipboardZenHanConverter.Core.Interfaces;
using EsUtil.ClipboardZenHanConverter.Core.Models;
using EsUtil.ClipboardZenHanConverter.Presentation.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace EsUtil.ClipboardZenHanConverter.App.AvaloniaUI.ViewModels;

/// <summary>ホーム画面のデータを管理し、クリップボード監視と文字変換を実行します。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - クリップボードの内容変更監視と自動文字変換<br/>
/// - 変換結果のクリップボードへの書き戻し<br/>
/// - 変換中の再帰的イベント防止（_isUpdatingClipboard フラグ + SemaphoreSlim）<br/><br/>
/// 特徴: <br/>
/// - IClipboardService / ITextConverter インターフェースに依存（DIP）<br/>
/// - テスト時は各インターフェースのモック実装で差し替え可能<br/>
/// - SemaphoreSlim による排他制御でクリップボード変更イベントの重複処理を防止<br/>
/// - クリップボード変換が無効（AppSetting.IsClipboardConvertEnabled が false）の場合は読み書きを一切行わない<br/>
/// - 全角/半角変換は常に有効。切り替えできるのはクリップボード連携（コピー検知・書き戻し）のみ<br/>
/// - プリセット選択はタイトルバーが単一所有し、この ViewModel は関与しない<br/><br/>
/// 処理フロー: <br/>
/// 1. クリップボード変更イベント受信<br/>
/// 2. 排他制御（SemaphoreSlim.WaitAsync(0)）<br/>
/// 3. BeforeText 更新<br/>
/// 4. ITextConverter.Convert で変換実行<br/>
/// 5. 変換結果が元のテキストと異なる場合のみクリップボードに書き戻し<br/>
/// 6. _isUpdatingClipboard フラグで書き戻し中の再帰イベントを防止
/// </remarks>
public partial class HomeViewModel : ObservableObject, IDisposable
{
    /// <summary>テキスト変換器（DIP: ITextConverter）。</summary>
    private readonly ITextConverter _converter;

    /// <summary>クリップボードサービス（DIP: IClipboardService）。</summary>
    private readonly IClipboardService _clipboardService;

    /// <summary>アプリ設定。クリップボード変換の有効/無効を参照します。</summary>
    private readonly AppSetting _appSetting;

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

    /// <summary>HomeViewModel の新しいインスタンスを初期化します。</summary>
    /// <param name="converter">テキスト変換器。</param>
    /// <param name="clipboardService">クリップボードサービス。</param>
    /// <param name="appSetting">アプリ設定。クリップボード変換の有効/無効を参照します。</param>
    public HomeViewModel(ITextConverter converter,
        IClipboardService clipboardService, AppSetting appSetting)
    {
        _converter = converter;
        _clipboardService = clipboardService;
        _appSetting = appSetting;
        _clipboardService.ContentChanged += Clipboard_ContentChanged;
    }

    /// <summary>クリップボードのイベント購読を解除します。</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _clipboardService.ContentChanged -= Clipboard_ContentChanged;
        _clipboardSemaphore.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>変換前テキストの変更時に呼び出され、変換後テキストを更新します。</summary>
    /// <param name="value">新しい変換前テキスト。</param>
    /// <remarks>クリップボード書き戻し中は再入を避けるため何もしません。<br/>
    /// 変更の要因（クリップボード取り込み・テキストボックスの編集）に関わらず変換を実行します。</remarks>
    partial void OnBeforeTextChanged(string value)
    {
        if (_isUpdatingClipboard) return;
        ConvertedText = ConvertForDisplay(value);
    }

    /// <summary>変換前テキストを表示用の変換後テキストへ変換します。</summary>
    /// <param name="text">変換前テキスト。</param>
    /// <returns>変換後テキスト。変化がない場合は「変換不要」の文言。</returns>
    /// <remarks>全角/半角変換は常に有効です（オン/オフの切り替えはありません）。</remarks>
    private string ConvertForDisplay(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        var converted = _converter.Convert(text);
        return text == converted ? HomeDisplayText.NoChange : converted;
    }

    /// <summary>クリップボードの内容変更時に呼び出されます。UI スレッドまたはバックグラウンドで変換処理を開始します。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">イベントデータ。</param>
    private void Clipboard_ContentChanged(object? sender, object e)
    {
        if (TestMode)
        {
            _ = HandleClipboardChangeAsync();
            return;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            _ = HandleClipboardChangeAsync();
        }
        else
        {
            Dispatcher.UIThread.Post(() => _ = HandleClipboardChangeAsync());
        }
    }

    /// <summary>クリップボード変更を非同期に処理します。</summary>
    /// <remarks>
    /// SemaphoreSlim で排他制御を行い、複数のイベントが同時に処理されるのを防ぎます。<br/>
    /// 処理フロー: <br/>
    /// 1. SemaphoreSlim で排他ロック取得（即時利用不可の場合はスキップ）<br/>
    /// 2. クリップボード変換が無効、または書き戻し中の場合はスキップ<br/>
    /// 3. クリップボードからテキスト取得<br/>
    /// 4. 変換結果が空または元と同じならスキップ<br/>
    /// 5. 変換結果をクリップボードに書き戻し<br/><br/>
    /// 注意点: <br/>
    /// - クリップボード変換が無効な場合、読み取りも書き戻しも行いません
    /// </remarks>
    private async Task HandleClipboardChangeAsync()
    {
        if (!await _clipboardSemaphore.WaitAsync(0))
            return;

        try
        {
            if (_isUpdatingClipboard || !_appSetting.IsClipboardConvertEnabled)
                return;

            var text = await _clipboardService.GetTextAsync();
            if (string.IsNullOrEmpty(text))
                return;

            if (ConvertedText == text)
                return;

            // await 中に無効化された場合は、読み取り結果を破棄してクリップボードに触れない。
            if (!_appSetting.IsClipboardConvertEnabled)
                return;

            BeforeText = text;
            if (ConvertedText == HomeDisplayText.NoChange)
                return;

            try
            {
                _isUpdatingClipboard = true;
                _clipboardService.SetText(ConvertedText);
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
            // クリップボード API の COM 相互運用例外も想定内。
        }
        finally
        {
            _clipboardSemaphore.Release();
        }
    }
}
