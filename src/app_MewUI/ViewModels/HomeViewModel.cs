using ClipboardZenHanConverter.Core.Interfaces;
using ClipboardZenHanConverter.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardZenHanConverter.App.MewUI.ViewModels;

/// <summary>ホーム画面のデータを管理し、クリップボード監視と文字変換を実行します。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - クリップボードの内容変更監視と自動文字変換（クリップボード変換が有効な場合のみ）<br/>
/// - 変換前テキストへの手動入力に対する変換<br/>
/// - 変換結果のクリップボードへの書き戻し<br/><br/>
/// 特徴: <br/>
/// - ITextConverter / IClipboardService / AppSetting に依存（DIP）<br/>
/// - クリップボード変換が無効な場合はクリップボードの読み取り・書き戻しを一切行いません<br/>
/// - 全角/半角変換は常に有効で、クリップボード変換の有効/無効とは独立です<br/>
/// - 書き戻し中の再帰イベントは _isUpdatingClipboard フラグで防止します
/// </remarks>
public partial class HomeViewModel : ObservableObject, IDisposable
{
    /// <summary>テキスト変換器（DIP: ITextConverter）。</summary>
    private readonly ITextConverter _converter;
    /// <summary>クリップボードサービス（DIP: IClipboardService）。</summary>
    private readonly IClipboardService _clipboardService;
    /// <summary>アプリ設定。クリップボード変換の有効/無効を参照します。</summary>
    private readonly AppSetting _appSetting;
    /// <summary>クリップボード書き戻し中フラグ（再帰防止）。</summary>
    private bool _isUpdatingClipboard;
    /// <summary>Dispose 済みフラグ。</summary>
    private bool _disposed;

    /// <summary>変換前のテキストを取得または設定します。</summary>
    /// <remarks>クリップボードからの取り込み、または変換前テキストボックスの編集で更新されます。<br/>
    /// 変更時は自動的に変換され、ConvertedText へ反映されます。</remarks>
    [ObservableProperty]
    public partial string BeforeText { get; set; } = "";

    /// <summary>変換後のテキストを取得または設定します。</summary>
    [ObservableProperty]
    public partial string ConvertedText { get; set; } = "";

    /// <summary>HomeViewModel の新しいインスタンスを初期化します。</summary>
    /// <param name="converter">テキスト変換器。</param>
    /// <param name="clipboardService">クリップボードサービス。</param>
    /// <param name="appSetting">アプリ設定。クリップボード変換の有効/無効を参照します。</param>
    public HomeViewModel(ITextConverter converter, IClipboardService clipboardService, AppSetting appSetting)
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

    /// <summary>クリップボードの内容変更時に呼び出され、変換処理を開始します。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">イベントデータ。</param>
    private async void Clipboard_ContentChanged(object? sender, object e)
    {
        try
        {
            await HandleClipboardChangeAsync();
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or UnauthorizedAccessException)
        {
            // バックグラウンド時などクリップボードアクセスが拒否されるケースは想定内の通常動作。
        }
    }

    /// <summary>クリップボード変更を処理します。</summary>
    /// <remarks>
    /// 処理フロー: <br/>
    /// 1. クリップボード変換が無効、または書き戻し中の場合はスキップ<br/>
    /// 2. クリップボードからテキスト取得<br/>
    /// 3. 変換後テキストと同一ならスキップ（書き戻し済みの値を再処理しない）<br/>
    /// 4. 変換前テキストを更新（変換後テキストは自動更新）<br/>
    /// 5. 変換結果が変化していればクリップボードへ書き戻し<br/><br/>
    /// 注意点: <br/>
    /// - クリップボード変換が無効な場合、読み取りも書き戻しも行いません
    /// </remarks>
    private async Task HandleClipboardChangeAsync()
    {
        if (_isUpdatingClipboard || !_appSetting.IsClipboardConvertEnabled)
            return;

        var text = await _clipboardService.GetTextAsync();
        if (string.IsNullOrEmpty(text) || ConvertedText == text)
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

    /// <summary>変換前テキストを表示用の変換後テキストへ変換します。</summary>
    /// <param name="text">変換前テキスト。</param>
    /// <returns>変換後テキスト。変化がない場合は「変換不要」の文言。</returns>
    private string ConvertForDisplay(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        var converted = _converter.Convert(text);
        return text == converted ? HomeDisplayText.NoChange : converted;
    }
}
