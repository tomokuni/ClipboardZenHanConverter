using EsUtil.ClipboardZenHanConverter.App.AvaloniaUI.ViewModels;
using EsUtil.ClipboardZenHanConverter.Core.Enums;
using EsUtil.ClipboardZenHanConverter.Core.Interfaces;
using EsUtil.ClipboardZenHanConverter.Core.Logic;
using EsUtil.ClipboardZenHanConverter.Core.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EsUtil.ClipboardZenHanConverter.Tests.App.AvaloniaUI.ViewModels;

/// <summary>テスト用のクリップボードサービススタブ。</summary>
file sealed class StubClipboardService : IClipboardService
{
    /// <summary>取得対象のテキスト。</summary>
    public string? Text { get; set; }

    /// <summary>最後に設定されたテキスト。</summary>
    public string? SetTextArg { get; private set; }

    /// <summary>Flush が呼び出されたかどうか。</summary>
    public bool FlushCalled { get; private set; }

    /// <summary>クリップボード内容変更イベント。</summary>
    public event EventHandler<object>? ContentChanged;

    /// <summary>クリップボードからテキストを取得します。</summary>
    /// <returns>設定されたテキスト。</returns>
    public Task<string?> GetTextAsync() => Task.FromResult(Text);

    /// <summary>テキストをクリップボードへ設定します。</summary>
    /// <param name="text">設定するテキスト。</param>
    public void SetText(string text) => SetTextArg = text;

    /// <summary>クリップボードをフラッシュします。</summary>
    public void Flush() => FlushCalled = true;

    /// <summary>リソースを解放します。</summary>
    public void Dispose()
    {
    }

    /// <summary>ContentChanged イベントを発行します。</summary>
    public void SimulateContentChange() => ContentChanged?.Invoke(this, EventArgs.Empty);
}

/// <summary><see cref="HomeViewModel"/> の表示用変換とクリップボード連携を検証します。</summary>
public sealed class HomeViewModelTests
{
    /// <summary>クリップボード変換を有効にしたアプリ設定（既定値）を作成します。</summary>
    /// <returns>クリップボード変換が有効な AppSetting。</returns>
    private static AppSetting CreateAppSetting() => new() { IsClipboardConvertEnabled = true };

    [Fact]
    public void Clipboard_ContentChanged_ConvertsAndSetsText()
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
        // 全角/半角変換は常に有効です（IsEnabledZenHan は画面側で参照しません。他 UI と同一）。
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
        // クリップボード変換トグルが無効な場合は、読み取りも書き戻しも行いません（他 UI と同一）。
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

    [Fact]
    public void BeforeText_変更時は変換後テキストを更新しクリップボードへ書き戻さない()
    {
        var clipboard = new StubClipboardService();
        ConvertConfig config = new() { ConvertModeAlphabet = ZenHanMode.ToHan };

        var vm = new HomeViewModel(new CharConverter(config), clipboard, CreateAppSetting())
        {
            BeforeText = "ＡＢＣ",
        };

        Assert.Equal("ABC", vm.ConvertedText);
        Assert.Null(clipboard.SetTextArg);
    }

    [Fact]
    public void BeforeText_変化がない場合は変換不要を表示する()
    {
        var clipboard = new StubClipboardService();
        ConvertConfig config = new() { ConvertModeAlphabet = ZenHanMode.ToHan };

        var vm = new HomeViewModel(new CharConverter(config), clipboard, CreateAppSetting())
        {
            BeforeText = "ABC",
        };

        Assert.Equal("変換不要 (変更なし)", vm.ConvertedText);
    }

    [Fact]
    public void BeforeText_空文字の場合は空文字を表示する()
    {
        var clipboard = new StubClipboardService();
        ConvertConfig config = new();

        var vm = new HomeViewModel(new CharConverter(config), clipboard, CreateAppSetting())
        {
            BeforeText = "",
        };

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
