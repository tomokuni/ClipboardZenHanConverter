using ClipboardZenHanConverter.Core.Logic;
using ClipboardZenHanConverter.Core.Models;
using ClipboardZenHanConverter.Core.Interfaces;
using ClipboardZenHanConverter.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardZenHanConverter.ViewModels;

/// <summary>ホーム画面のデータを管理するViewModelクラスです。</summary>
public partial class HomeViewModel : ObservableObject, IDisposable
{
    private readonly CharConverter _converter;
    private readonly IClipboardService _clipboardService;
    private readonly INavigationService _navigation;
    private readonly DispatcherQueue? _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    private readonly SemaphoreSlim _clipboardSemaphore = new(1, 1);
    private bool _isUpdatingClipboard;
    private bool _disposed;

    /// <summary>テスト用: trueに設定すると同期実行します。</summary>
    public bool TestMode { get; set; }

    [ObservableProperty]
    public partial string BeforeText { get; set; } = "";

    [ObservableProperty]
    public partial string ConvertedText { get; set; } = "";

    public ConvertConfig Config { get; }

    [RelayCommand]
    private void NavigateToSettings() => _navigation.NavigateTo("Settings");

    public HomeViewModel(ConvertConfig config, CharConverter converter,
        IClipboardService clipboardService, INavigationService navigation)
    {
        Config = config;
        _converter = converter;
        _clipboardService = clipboardService;
        _navigation = navigation;
        _clipboardService.ContentChanged += Clipboard_ContentChanged;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _clipboardService.ContentChanged -= Clipboard_ContentChanged;
        _clipboardSemaphore.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

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
