using EsUtil.ClipboardZenHanConverter.App.MewUI.ViewModels;
using EsUtil.ClipboardZenHanConverter.Core.Enums;
using EsUtil.ClipboardZenHanConverter.Core.Interfaces;
using EsUtil.ClipboardZenHanConverter.Core.Logic;
using EsUtil.ClipboardZenHanConverter.Core.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EsUtil.ClipboardZenHanConverter.Tests.App.MewUI.ViewModels;

/// <summary><see cref="HomeViewModel"/> の表示変換とクリップボード連携を検証します。</summary>
public sealed class HomeViewModelTests : IDisposable
{
    /// <summary>変換結果が元テキストと同一の場合に表示される文言。</summary>
    private const string NoChangeText = "変換不要 (変更なし)";

    private readonly ConvertConfig _config = TestHelper.CreateDefaultConfig();
    private readonly CharConverter _converter;
    private readonly FakeClipboardService _clipboard = new();
    private readonly AppSetting _appSetting = new();
    private readonly HomeViewModel _viewModel;

    public HomeViewModelTests()
    {
        _converter = new CharConverter(_config);
        _viewModel = new HomeViewModel(_converter, _clipboard, _appSetting);
    }

    public void Dispose()
    {
        _viewModel.Dispose();
        _converter.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void BeforeText_変更で変換後テキストを更新する()
    {
        _config.ConvertModeNumber = ZenHanMode.ToHan;

        _viewModel.BeforeText = "１２３";

        Assert.Equal("123", _viewModel.ConvertedText);
    }

    [Fact]
    public void BeforeText_変換結果が同一の場合は変換不要を表示する()
    {
        _config.ConvertModeNumber = ZenHanMode.None;

        _viewModel.BeforeText = "abc";

        Assert.Equal(NoChangeText, _viewModel.ConvertedText);
    }

    [Fact]
    public void BeforeText_空文字の場合は空文字を返す()
    {
        _viewModel.BeforeText = string.Empty;

        Assert.Equal(string.Empty, _viewModel.ConvertedText);
    }

    [Theory]
    [InlineData(ConvertConfig.BuiltInPresetAccountingPower, "２０２４年１２月３１日（金）", "2024年12月31日(金)")]
    [InlineData(ConvertConfig.BuiltInPresetAccountingPower, "（株）ＡＢＣ商事 １，２３４，５６７円", "(株)ABC商事 1,234,567円")]
    [InlineData(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen, "ＴＥＬ：０３−１２３４−５６７８（代表）", "TEL:03-1234-5678(代表)")]
    [InlineData(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen, "ﾀﾞｲｽｷﾅﾗｰﾒﾝ! ﾏｲｳｪｲ!", "ダイスキナラーメン! マイウェイ!")]
    public void BeforeText_組み込みプリセットの変換結果がcoreと一致する(string presetName, string input, string expected)
    {
        // 画面側の経路（BeforeText 変更 → 自動変換）でも、共有 Core と同じ変換結果になることを確認します。
        _config.LoadPreset(presetName);

        _viewModel.BeforeText = input;

        Assert.Equal(expected, _viewModel.ConvertedText);
    }

    [Fact]
    public async Task Clipboard変換が無効の場合は読み書きしない()
    {
        _appSetting.IsClipboardConvertEnabled = false;
        _config.ConvertModeNumber = ZenHanMode.ToHan;
        _clipboard.Text = "１２３";

        _clipboard.RaiseContentChanged();
        await Task.Delay(100, TestContext.Current.CancellationToken);

        Assert.Equal(0, _clipboard.GetTextCount);
        Assert.Equal(0, _clipboard.SetTextCount);
    }

    [Fact]
    public async Task Clipboard変換が有効の場合は変換して書き戻す()
    {
        _appSetting.IsClipboardConvertEnabled = true;
        _config.ConvertModeNumber = ZenHanMode.ToHan;
        _clipboard.Text = "１２３";

        _clipboard.RaiseContentChanged();
        var completed = await TestHelper.WaitUntilAsync(() => _clipboard.SetTextCount == 1);

        Assert.True(completed);
        Assert.Equal("123", _clipboard.LastSetText);
        Assert.Equal("１２３", _viewModel.BeforeText);
        Assert.Equal(1, _clipboard.FlushCount);
    }

    [Fact]
    public async Task Clipboard変換結果が同一の場合は書き戻さない()
    {
        _appSetting.IsClipboardConvertEnabled = true;
        _config.ConvertModeNumber = ZenHanMode.None;
        _clipboard.Text = "abc";

        _clipboard.RaiseContentChanged();
        var read = await TestHelper.WaitUntilAsync(() => _clipboard.GetTextCount == 1);
        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.True(read);
        Assert.Equal(0, _clipboard.SetTextCount);
    }

    [Fact]
    public async Task Clipboard_変換後テキストと同一の内容は書き戻さない()
    {
        _appSetting.IsClipboardConvertEnabled = true;
        _config.ConvertModeNumber = ZenHanMode.ToHan;
        _viewModel.ConvertedText = "123";
        _clipboard.Text = "123";

        _clipboard.RaiseContentChanged();
        var read = await TestHelper.WaitUntilAsync(() => _clipboard.GetTextCount == 1);
        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.True(read);
        Assert.Equal(0, _clipboard.SetTextCount);
        Assert.Equal(string.Empty, _viewModel.BeforeText);
    }

    [Fact]
    public async Task Clipboard_空のテキストは処理しない()
    {
        _appSetting.IsClipboardConvertEnabled = true;
        _clipboard.Text = null;

        _clipboard.RaiseContentChanged();
        var read = await TestHelper.WaitUntilAsync(() => _clipboard.GetTextCount == 1);
        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.True(read);
        Assert.Equal(0, _clipboard.SetTextCount);
    }

    [Fact]
    public async Task Dispose_イベント購読を解除する()
    {
        _appSetting.IsClipboardConvertEnabled = true;
        _viewModel.Dispose();

        _clipboard.RaiseContentChanged();
        await Task.Delay(100, TestContext.Current.CancellationToken);

        Assert.Equal(0, _clipboard.GetTextCount);
    }

    [Fact]
    public void Dispose_複数回呼び出しても例外を送出しない()
    {
        _viewModel.Dispose();

        var ex = Record.Exception(() => _viewModel.Dispose());

        Assert.Null(ex);
    }

    /// <summary>呼び出し回数を記録するクリップボードサービスのスタブ。</summary>
    private sealed class FakeClipboardService : IClipboardService
    {
        /// <summary>取得対象のテキスト。</summary>
        public string? Text { get; set; }

        /// <summary>最後に設定されたテキスト。</summary>
        public string? LastSetText { get; private set; }

        /// <summary>GetTextAsync の呼び出し回数。</summary>
        public int GetTextCount { get; private set; }

        /// <summary>SetText の呼び出し回数。</summary>
        public int SetTextCount { get; private set; }

        /// <summary>Flush の呼び出し回数。</summary>
        public int FlushCount { get; private set; }

        /// <inheritdoc/>
        public event EventHandler<object>? ContentChanged;

        /// <inheritdoc/>
        public Task<string?> GetTextAsync()
        {
            GetTextCount++;
            return Task.FromResult(Text);
        }

        /// <inheritdoc/>
        public void SetText(string text)
        {
            SetTextCount++;
            LastSetText = text;
        }

        /// <inheritdoc/>
        public void Flush() => FlushCount++;

        /// <summary>クリップボード内容変更イベントを発火します。</summary>
        public void RaiseContentChanged() => ContentChanged?.Invoke(this, new object());

        /// <inheritdoc/>
        public void Dispose() { }
    }
}
