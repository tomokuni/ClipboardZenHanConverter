using ClipboardZenHanConverter.App.ViewModels;
using ClipboardZenHanConverter.Core.Enums;
using ClipboardZenHanConverter.Core.Interfaces;
using ClipboardZenHanConverter.Core.Logic;
using ClipboardZenHanConverter.Core.Models;
using Xunit;

namespace ClipboardZenHanConverter.Tests;

/// <summary>テスト用のクリップボードサービススタブ。</summary>
file sealed partial class StubClipboardService : IClipboardService
{
    public string? Text { get; set; }
    public string? SetTextArg { get; private set; }
    public bool FlushCalled { get; private set; }

    public event EventHandler<object>? ContentChanged;

    public Task<string?> GetTextAsync() => Task.FromResult(Text);
    public void SetText(string text) => SetTextArg = text;
    public void Flush() => FlushCalled = true;
    public void Dispose() { }
    public void SimulateContentChange() => ContentChanged?.Invoke(this, EventArgs.Empty);
}

/// <summary>テスト用のナビゲーションサービススタブ。</summary>
file sealed class StubNavigationService : INavigationService
{
    public object? LastNavigateTo { get; private set; }
    public void NavigateTo(object? page) => LastNavigateTo = page;
    public void Initialize() { }
    public void PreloadSettingsAsync() { }
}

public class HomeViewModelTests
{
    [Fact]
    public async Task Clipboard_ContentChanged_ConvertsAndSetsText()
    {
        var clipboard = new StubClipboardService { Text = "ＡＢＣ" };
        var navigation = new StubNavigationService();
        ConvertConfig config = new() { IsEnabledZenHan = true, ConvertModeAlphabet = ZenHanMode.ToHan };

        CharConverter converter = new(config);
        var vm = new HomeViewModel(config, converter, clipboard, navigation);
        vm.TestMode = true;
        clipboard.SimulateContentChange();

        Assert.Equal("ABC", clipboard.SetTextArg);
        Assert.True(clipboard.FlushCalled);
    }

    [Fact]
    public void Clipboard_ContentChanged_DoesNotSetText_IfNoChange()
    {
        var clipboard = new StubClipboardService { Text = "ABC" };
        var navigation = new StubNavigationService();
        ConvertConfig config = new() { IsEnabledZenHan = true, ConvertModeAlphabet = ZenHanMode.ToHan };

        var vm = new HomeViewModel(config, new CharConverter(config), clipboard, navigation);
        vm.TestMode = true;
        clipboard.SimulateContentChange();

        Assert.Null(clipboard.SetTextArg);
    }
}

