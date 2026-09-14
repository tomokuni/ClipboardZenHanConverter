using EsUtil.ClipboardZenHanConverter.Core.Enums;
using EsUtil.ClipboardZenHanConverter.Core.Logic;
using EsUtil.ClipboardZenHanConverter.Core.Models;
using System;
using System.Threading.Tasks;
using Xunit;
using MewUIHomeViewModel = EsUtil.ClipboardZenHanConverter.App.MewUI.ViewModels.HomeViewModel;
using WinFormsHomeViewModel = EsUtil.ClipboardZenHanConverter.App.WinForms.ViewModels.HomeViewModel;
using WinUIHomeViewModel = EsUtil.ClipboardZenHanConverter.App.WinUI.ViewModels.HomeViewModel;

namespace EsUtil.ClipboardZenHanConverter.Tests.Parity;

/// <summary>WinForms 版の挙動が MewUI 版・WinUI 版と一致することを検証します。</summary>
/// <remarks>
/// WinForms 版の ViewModel は他 UI の実装を基にしているため、共有コアの変更に追随し忘れると
/// 見た目は同じなのに変換結果が異なる状態になります。同じ入力に対する 3 実装の出力を直接比較し、その状態を検出します。<br/><br/>
/// 前提: クリップボード変換トグル（<c>AppSetting.IsClipboardConvertEnabled</c>）は 3 実装とも有効にして比較します。
/// </remarks>
public sealed class WinFormsParityTests : IDisposable
{
    /// <summary>変換結果が元テキストと同一の場合に各実装が表示する文言。</summary>
    private const string NoChangeText = "変換不要 (変更なし)";

    /// <summary>各実装が共有する変換設定。</summary>
    private readonly ConvertConfig _config = TestHelper.CreateDefaultConfig();

    /// <summary>MewUI 版のクリップボードサービス。</summary>
    private readonly FakeClipboardService _mewClipboard = new();

    /// <summary>WinUI 版のクリップボードサービス。</summary>
    private readonly FakeClipboardService _winClipboard = new();

    /// <summary>WinForms 版のクリップボードサービス。</summary>
    private readonly FakeClipboardService _formsClipboard = new();

    private readonly MewUIHomeViewModel _mewViewModel;
    private readonly WinUIHomeViewModel _winViewModel;
    private readonly WinFormsHomeViewModel _formsViewModel;

    public WinFormsParityTests()
    {
        _mewViewModel = new MewUIHomeViewModel(
            new CharConverter(_config), _mewClipboard, new AppSetting { IsClipboardConvertEnabled = true });
        _winViewModel = new WinUIHomeViewModel(
            new CharConverter(_config), _winClipboard, new AppSetting { IsClipboardConvertEnabled = true })
        {
            TestMode = true,
        };
        _formsViewModel = new WinFormsHomeViewModel(
            new CharConverter(_config), _formsClipboard, new AppSetting { IsClipboardConvertEnabled = true })
        {
            TestMode = true,
        };
    }

    public void Dispose()
    {
        _mewViewModel.Dispose();
        _winViewModel.Dispose();
        _formsViewModel.Dispose();
        GC.SuppressFinalize(this);
    }

    [Theory]
    [InlineData(ConvertConfig.BuiltInPresetAccountingPower, "２０２４年１２月３１日（金）", "2024年12月31日(金)")]
    [InlineData(ConvertConfig.BuiltInPresetAccountingPower, "（株）ＡＢＣ商事 １，２３４，５６７円", "(株)ABC商事 1,234,567円")]
    [InlineData(ConvertConfig.BuiltInPresetAccountingPower, "〒１００−０００１ 東京ﾄｳｷｮｳ", "〒100-0001 東京トウキョウ")]
    [InlineData(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen, "（株）ＡＢＣ商事「設定」２０２４年", "(株)ABC商事｢設定｣2024年")]
    [InlineData(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen, "ＴＥＬ：０３−１２３４−５６７８（代表）", "TEL:03-1234-5678(代表)")]
    [InlineData(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen, "ﾀﾞｲｽｷﾅﾗｰﾒﾝ! ﾏｲｳｪｲ!", "ダイスキナラーメン! マイウェイ!")]
    public async Task 表示用変換が3実装で一致する(string presetName, string input, string expected)
    {
        _config.LoadPreset(presetName);

        SetClipboardTextFromAll(input);
        await WaitAllConvertedTextAsync(expected);

        Assert.Equal(expected, _mewViewModel.ConvertedText);
        Assert.Equal(expected, _winViewModel.ConvertedText);
        Assert.Equal(expected, _formsViewModel.ConvertedText);
    }

    [Theory]
    [InlineData("ABC")]
    [InlineData("ＡＢＣ")]
    [InlineData("漢字かな")]
    public async Task 変化しない入力では3実装とも変換不要を表示する(string input)
    {
        // 変換モードを全て None にし、変化しない入力を与えます。
        _config.ConvertModeNumber = ZenHanMode.None;

        SetClipboardTextFromAll(input);
        await WaitAllConvertedTextAsync(NoChangeText);

        Assert.Equal(NoChangeText, _mewViewModel.ConvertedText);
        Assert.Equal(NoChangeText, _winViewModel.ConvertedText);
        Assert.Equal(NoChangeText, _formsViewModel.ConvertedText);
    }

    [Fact]
    public async Task 全角半角変換は3実装とも常に有効()
    {
        // 変換の実行可否を切り替える UI は 3 実装とも持ちません（IsEnabledZenHan は参照しません）。
        _config.IsEnabledZenHan = false;
        _config.ConvertModeAlphabet = ZenHanMode.ToHan;

        SetClipboardTextFromAll("ＡＢＣ");
        await WaitAllConvertedTextAsync("ABC");

        Assert.Equal("ABC", _mewViewModel.ConvertedText);
        Assert.Equal("ABC", _winViewModel.ConvertedText);
        Assert.Equal("ABC", _formsViewModel.ConvertedText);
    }

    [Fact]
    public async Task クリップボード書き戻しの内容が3実装で一致する()
    {
        _config.ConvertModeAlphabet = ZenHanMode.ToHan;
        _config.ConvertModeNumber = ZenHanMode.ToHan;

        SetClipboardTextFromAll("ＡＢＣ１２３");
        await WaitAllConvertedTextAsync("ABC123");

        Assert.Equal("ABC123", _mewClipboard.LastSetText);
        Assert.Equal("ABC123", _winClipboard.LastSetText);
        Assert.Equal("ABC123", _formsClipboard.LastSetText);
    }

    /// <summary>3 実装のクリップボードへ同じテキストを設定し、変更を通知します。</summary>
    /// <param name="text">設定するテキスト。</param>
    private void SetClipboardTextFromAll(string text)
    {
        _mewClipboard.Text = text;
        _winClipboard.Text = text;
        _formsClipboard.Text = text;

        _mewClipboard.RaiseContentChanged();
        _winClipboard.RaiseContentChanged();
        _formsClipboard.RaiseContentChanged();
    }

    /// <summary>3 実装すべての変換結果が期待値になるまで待機します。</summary>
    /// <param name="expected">期待する変換結果。</param>
    /// <returns>待機タスク。</returns>
    private async Task WaitAllConvertedTextAsync(string expected)
        => await TestHelper.WaitUntilAsync(
            () => _mewViewModel.ConvertedText == expected
                && _winViewModel.ConvertedText == expected
                && _formsViewModel.ConvertedText == expected);
}
