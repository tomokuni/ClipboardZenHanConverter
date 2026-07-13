using ClipboardZenHanConverter.Core.Logic;
using ClipboardZenHanConverter.Core.Models;
using ClipboardZenHanConverter.Core.Services;
using ClipboardZenHanConverter.Services;
using ClipboardZenHanConverter.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardZenHanConverter.ViewModels;

/// <summary>ホーム画面のデータを管理するViewModelクラスです。</summary>
/// <remarks>
/// ボタンのテキスト表示とクリックイベントの処理を提供します。<br/>
/// <br/>
/// </remarks>
public partial class HomeViewModel : ObservableObject, IDisposable
{
    private readonly IClipboardService _clipboardService;
    private readonly CharConverter _converter;
    private readonly INavigationService _navigation;
    private readonly DispatcherQueue? _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    [ObservableProperty]
    public partial string BeforeText { get; set; } = "";

    [ObservableProperty]
    public partial string ConvertedText { get; set; } = "";

    // 非同期処理の排他制御用 SemaphoreSlim（複数イベントの同時実行を防止）
    private readonly SemaphoreSlim _clipboardSemaphore = new(1, 1);

    // Re-entrancy flag to prevent infinite loops when updating clipboard
    private bool _isUpdatingClipboard;

    public ConvertConfig Config { get; }

    /// <summary>設定画面への遷移コマンドです。</summary>
    [RelayCommand]
    private void NavigateToSettings() => _navigation.NavigateTo("Settings");

    public HomeViewModel(ConvertConfig config, CharConverter converter, IClipboardService clipboardService, INavigationService navigation)
    {
        Config = config;
        _converter = converter;
        _clipboardService = clipboardService;
        _navigation = navigation;
        
        _clipboardService.ContentChanged += Clipboard_ContentChanged;
    }


    private bool _disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _clipboardService.ContentChanged -= Clipboard_ContentChanged;
            _clipboardSemaphore.Dispose();
        }

        _disposed = true;
    }

    /// <summary>クリップボードの内容が変更されたときに呼び出され、テキストデータがあれば画面に表示します。</summary>
    /// <param name="sender">イベント送信元。</param>
    /// <param name="e">イベント引数。</param>
    private void Clipboard_ContentChanged(object? sender, object e)
    {
        // クリップボードイベントは別スレッドで発生する可能性があるためUIスレッドにマーシャリングする
        if (_dispatcherQueue != null)
        {
            _dispatcherQueue.TryEnqueue(async () =>
            {
                await HandleClipboardChangeAsync();
            });
        }
        else
        {
            // UnitTest などで DispatcherQueue が未初期化の場合は Task.Run でバックグラウンド実行する
            _ = Task.Run(async () => await HandleClipboardChangeAsync());
        }
    }


    private async Task HandleClipboardChangeAsync()
    {
        // SemaphoreSlim で排他制御（複数のイベントが同時に処理されるのを防ぐ）
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
        catch (Exception)
        {
            // クリップボードが他のプロセスによってロックされている場合や、
            // アプリがバックグラウンドにありアクセスが拒否された場合は無視します。
        }
        finally
        {
            _clipboardSemaphore.Release();
        }
    }
}
