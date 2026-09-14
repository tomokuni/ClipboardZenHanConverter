using ClipboardZenHanConverter.App.WinUI.ViewModels;
using ClipboardZenHanConverter.Core.Enums;
using ClipboardZenHanConverter.Core.Interfaces;
using ClipboardZenHanConverter.Core.Logic;
using ClipboardZenHanConverter.Core.Models;
using Xunit;

namespace ClipboardZenHanConverter.Tests.App.WinUI.ViewModels;

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

public class HomeViewModelTests
{
    /// <summary>クリップボード変換を有効にしたアプリ設定（既定値）。</summary>
    private static AppSetting CreateAppSetting() => new() { IsClipboardConvertEnabled = true };

    [Fact]
    public async Task Clipboard_ContentChanged_ConvertsAndSetsText()
    {
        var clipboard = new StubClipboardService { Text = "ＡＢＣ" };
        ConvertConfig config = new() { ConvertModeAlphabet = ZenHanMode.ToHan };

        CharConverter converter = new(config);
        var vm = new HomeViewModel(converter, clipboard, CreateAppSetting());
        vm.TestMode = true;
        clipboard.SimulateContentChange();

        Assert.Equal("ABC", clipboard.SetTextArg);
        Assert.True(clipboard.FlushCalled);
    }

    [Fact]
    public void Clipboard_ContentChanged_DoesNotSetText_IfNoChange()
    {
        var clipboard = new StubClipboardService { Text = "ABC" };
        ConvertConfig config = new() { ConvertModeAlphabet = ZenHanMode.ToHan };

        var vm = new HomeViewModel(new CharConverter(config), clipboard, CreateAppSetting());
        vm.TestMode = true;
        clipboard.SimulateContentChange();

        Assert.Null(clipboard.SetTextArg);
    }

    [Fact]
    public void Clipboard_ContentChanged_ConvertsEvenIfZenHanFlagDisabled()
    {
        // 全角/半角変換は常に有効です（IsEnabledZenHan は画面側で参照しません。MewUI 版と同一）。
        var clipboard = new StubClipboardService { Text = "ＡＢＣ" };
        ConvertConfig config = new() { IsEnabledZenHan = false, ConvertModeAlphabet = ZenHanMode.ToHan };

        var vm = new HomeViewModel(new CharConverter(config), clipboard, CreateAppSetting());
        vm.TestMode = true;
        clipboard.SimulateContentChange();

        Assert.Equal("ABC", clipboard.SetTextArg);
    }

    [Fact]
    public void Clipboard_ContentChanged_DoesNothing_IfClipboardConvertDisabled()
    {
        // クリップボード変換トグルが無効な場合は、読み取りも書き戻しも行いません（MewUI 版と同一）。
        var clipboard = new StubClipboardService { Text = "ＡＢＣ" };
        ConvertConfig config = new() { ConvertModeAlphabet = ZenHanMode.ToHan };

        var vm = new HomeViewModel(new CharConverter(config), clipboard,
            new AppSetting { IsClipboardConvertEnabled = false })
        {
            TestMode = true,
        };
        clipboard.SimulateContentChange();

        Assert.Null(clipboard.SetTextArg);
        Assert.False(clipboard.FlushCalled);
        Assert.Equal(string.Empty, vm.BeforeText);
        Assert.Equal(string.Empty, vm.ConvertedText);
    }

    [Theory]
    [InlineData(ConvertConfig.BuiltInPresetAccountingPower, "２０２４年１２月３１日（金）", "2024年12月31日(金)")]
    [InlineData(ConvertConfig.BuiltInPresetAccountingPower, "（株）ＡＢＣ商事 １，２３４，５６７円", "(株)ABC商事 1,234,567円")]
    [InlineData(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen, "ＴＥＬ：０３−１２３４−５６７８（代表）", "TEL:03-1234-5678(代表)")]
    [InlineData(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen, "ﾀﾞｲｽｷﾅﾗｰﾒﾝ! ﾏｲｳｪｲ!", "ダイスキナラーメン! マイウェイ!")]
    public void Clipboard_組み込みプリセットの変換結果がcoreと一致する(string presetName, string input, string expected)
    {
        // 画面側の経路（クリップボード変更 → 自動変換 → 書き戻し）でも、共有 Core と同じ変換結果になることを確認します。
        var clipboard = new StubClipboardService { Text = input };
        ConvertConfig config = new();
        config.LoadPreset(presetName);

        var vm = new HomeViewModel(new CharConverter(config), clipboard, CreateAppSetting());
        vm.TestMode = true;
        clipboard.SimulateContentChange();

        Assert.Equal(expected, clipboard.SetTextArg);
    }
}
