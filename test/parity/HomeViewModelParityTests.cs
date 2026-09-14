using EsUtil.ClipboardZenHanConverter.Core.Enums;
using EsUtil.ClipboardZenHanConverter.Core.Logic;
using EsUtil.ClipboardZenHanConverter.Core.Models;
using System;
using System.Threading.Tasks;
using Xunit;
using MewUIHomeViewModel = EsUtil.ClipboardZenHanConverter.App.MewUI.ViewModels.HomeViewModel;
using WinUIHomeViewModel = EsUtil.ClipboardZenHanConverter.App.WinUI.ViewModels.HomeViewModel;

namespace EsUtil.ClipboardZenHanConverter.Tests.Parity;

/// <summary>MewUI 版と WinUI 版のホーム画面の挙動が一致することを検証します。</summary>
/// <remarks>
/// 両アプリは共有コア（CharConverter）を使いますが、ViewModel は別実装です。<br/>
/// 同じ入力に対する <c>BeforeText</c> / <c>ConvertedText</c> / クリップボード書き戻しを
/// 直接比較することで、実装のずれを検出します。<br/><br/>
/// 前提: MewUI 版のクリップボード変換トグル（<c>AppSetting.IsClipboardConvertEnabled</c>）は有効にして比較します。<br/>
/// このトグルは MewUI 版のみが持つ機能のためです。
/// </remarks>
public sealed class HomeViewModelParityTests : IDisposable
{
    /// <summary>変換結果が元テキストと同一の場合に両実装が表示する文言。</summary>
    private const string NoChangeText = "変換不要 (変更なし)";

    /// <summary>MewUI 版と WinUI 版が共有する変換設定。</summary>
    private readonly ConvertConfig _config = TestHelper.CreateDefaultConfig();

    /// <summary>MewUI 版のクリップボードサービス。</summary>
    private readonly FakeClipboardService _mewClipboard = new();

    /// <summary>WinUI 版のクリップボードサービス。</summary>
    private readonly FakeClipboardService _winClipboard = new();

    /// <summary>MewUI 版のアプリ設定（クリップボード変換を有効化）。</summary>
    private readonly AppSetting _mewAppSetting = new() { IsClipboardConvertEnabled = true };

    /// <summary>WinUI 版のアプリ設定（クリップボード変換を有効化）。</summary>
    private readonly AppSetting _winAppSetting = new() { IsClipboardConvertEnabled = true };

    private readonly MewUIHomeViewModel _mewViewModel;
    private readonly WinUIHomeViewModel _winViewModel;

    public HomeViewModelParityTests()
    {
        _mewViewModel = new MewUIHomeViewModel(new CharConverter(_config), _mewClipboard, _mewAppSetting);
        _winViewModel = new WinUIHomeViewModel(new CharConverter(_config), _winClipboard, _winAppSetting)
        {
            TestMode = true,
        };
    }

    public void Dispose()
    {
        _mewViewModel.Dispose();
        _winViewModel.Dispose();
        GC.SuppressFinalize(this);
    }

    [Theory]
    [InlineData(ConvertConfig.BuiltInPresetAccountingPower, "２０２４年１２月３１日（金）", "2024年12月31日(金)")]
    [InlineData(ConvertConfig.BuiltInPresetAccountingPower, "（株）ＡＢＣ商事 １，２３４，５６７円", "(株)ABC商事 1,234,567円")]
    [InlineData(ConvertConfig.BuiltInPresetAccountingPower, "〒１００−０００１ 東京ﾄｳｷｮｳ", "〒100-0001 東京トウキョウ")]
    [InlineData(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen, "（株）ＡＢＣ商事「設定」２０２４年", "(株)ABC商事｢設定｣2024年")]
    [InlineData(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen, "ＴＥＬ：０３−１２３４−５６７８（代表）", "TEL:03-1234-5678(代表)")]
    [InlineData(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen, "当社の製品「Ｗｉｄｇｅｔ」は￥１，２００です。", "当社の製品｢Widget｣は¥1,200です。")]
    [InlineData(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen, "ﾀﾞｲｽｷﾅﾗｰﾒﾝ! ﾏｲｳｪｲ!", "ダイスキナラーメン! マイウェイ!")]
    public async Task 表示用変換が一致する(string presetName, string input, string expected)
    {
        _config.LoadPreset(presetName);

        SetBothBeforeTextFromClipboard(input);
        await WaitBothConvertedTextAsync(expected);

        Assert.Equal(expected, _mewViewModel.ConvertedText);
        Assert.Equal(expected, _winViewModel.ConvertedText);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("ＡＢＣ")]
    [InlineData("漢字かな")]
    public async Task 変化しない入力では両実装とも変換不要を表示する(string input)
    {
        // 変換モードを全て None にし、変化しない入力を与えます。
        _config.ConvertModeNumber = ZenHanMode.None;

        SetBothBeforeTextFromClipboard(input);
        await WaitBothConvertedTextAsync(NoChangeText);

        Assert.Equal(NoChangeText, _mewViewModel.ConvertedText);
        Assert.Equal(NoChangeText, _winViewModel.ConvertedText);
    }

    [Theory]
    [InlineData("１２３", ZenHanMode.ToHan, "123")]
    [InlineData("123", ZenHanMode.ToZen, "１２３")]
    public async Task 個別モードの変換も一致する(string input, ZenHanMode mode, string expected)
    {
        _config.ConvertModeNumber = mode;

        SetBothBeforeTextFromClipboard(input);
        await WaitBothConvertedTextAsync(expected);

        Assert.Equal(expected, _mewViewModel.ConvertedText);
        Assert.Equal(expected, _winViewModel.ConvertedText);
    }

    [Fact]
    public async Task 全角半角変換は両実装とも常に有効()
    {
        // 変換の実行可否を切り替える UI は両実装とも持ちません（IsEnabledZenHan は参照しません）。
        _config.IsEnabledZenHan = false;
        _config.ConvertModeAlphabet = ZenHanMode.ToHan;

        SetBothBeforeTextFromClipboard("ＡＢＣ");
        await WaitBothConvertedTextAsync("ABC");

        Assert.Equal("ABC", _mewViewModel.ConvertedText);
        Assert.Equal("ABC", _winViewModel.ConvertedText);
    }

    [Fact]
    public async Task クリップボード変更時の書き戻しが一致する()
    {
        _config.ConvertModeAlphabet = ZenHanMode.ToHan;

        SetBothBeforeTextFromClipboard("ＡＢＣ");
        await WaitBothConvertedTextAsync("ABC");

        Assert.Equal("ABC", _mewClipboard.LastSetText);
        Assert.Equal("ABC", _winClipboard.LastSetText);
        Assert.Equal(1, _mewClipboard.SetTextCount);
        Assert.Equal(1, _winClipboard.SetTextCount);
        Assert.Equal(1, _mewClipboard.FlushCount);
        Assert.Equal(1, _winClipboard.FlushCount);
    }

    [Fact]
    public async Task 変化しない場合はクリップボードへ書き戻さない()
    {
        _config.ConvertModeAlphabet = ZenHanMode.None;

        SetBothBeforeTextFromClipboard("ABC");
        await WaitBothConvertedTextAsync(NoChangeText);

        Assert.Equal(0, _mewClipboard.SetTextCount);
        Assert.Equal(0, _winClipboard.SetTextCount);
        Assert.Equal(0, _mewClipboard.FlushCount);
        Assert.Equal(0, _winClipboard.FlushCount);
    }

    [Fact]
    public async Task 変換前テキストは両実装ともクリップボードの内容になる()
    {
        _config.ConvertModeAlphabet = ZenHanMode.ToHan;
        const string input = "ＡＢＣ";

        SetBothBeforeTextFromClipboard(input);
        await WaitBothConvertedTextAsync("ABC");

        Assert.Equal(input, _mewViewModel.BeforeText);
        Assert.Equal(input, _winViewModel.BeforeText);
    }

    [Fact]
    public async Task クリップボード変換が無効の場合は両実装とも読み書きしない()
    {
        _config.ConvertModeAlphabet = ZenHanMode.ToHan;
        _mewAppSetting.IsClipboardConvertEnabled = false;
        _winAppSetting.IsClipboardConvertEnabled = false;

        SetBothBeforeTextFromClipboard("ＡＢＣ");
        await Task.Delay(200, TestContext.Current.CancellationToken);

        Assert.Equal(0, _mewClipboard.SetTextCount);
        Assert.Equal(0, _winClipboard.SetTextCount);
        Assert.Equal(string.Empty, _mewViewModel.BeforeText);
        Assert.Equal(string.Empty, _winViewModel.BeforeText);
    }

    /// <summary>両実装へ同じ入力（クリップボードの内容）を与えます。</summary>
    /// <param name="input">クリップボードに設定するテキスト。</param>
    private void SetBothBeforeTextFromClipboard(string input)
    {
        _mewClipboard.Text = input;
        _winClipboard.Text = input;
        _mewClipboard.RaiseContentChanged();
        _winClipboard.RaiseContentChanged();
    }

    /// <summary>両実装の変換後テキストが指定値へ収束するまで待機します。</summary>
    /// <param name="expected">期待する変換後テキスト。</param>
    /// <remarks>タイムアウトした場合も後続の Assert で差分が報告されます。</remarks>
    private async Task WaitBothConvertedTextAsync(string expected)
        => await TestHelper.WaitUntilAsync(
            () => _mewViewModel.ConvertedText == expected && _winViewModel.ConvertedText == expected);
}
