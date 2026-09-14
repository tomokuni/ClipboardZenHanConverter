using ClipboardZenHanConverter.Core.Enums;
using ClipboardZenHanConverter.Core.Models;
using Xunit;

namespace ClipboardZenHanConverter.Tests.Core.Models;

public class ConvertConfigTests
{
    [Fact]
    public void Constructor_デフォルト値()
    {
        var config = new ConvertConfig();
        Assert.False(config.IsEnabledZenHan);
        Assert.Equal(ZenHanMode.None, config.ConvertModeNumber);
        Assert.Equal(ZenHanMode.None, config.ConvertModeAlphabet);
        Assert.Empty(config.ReplacePairs);
    }

    [Fact]
    public void IsBuiltInPreset_組込みプリセット名はtrue()
    {
        Assert.True(ConvertConfig.IsBuiltInPreset(ConvertConfig.BuiltInPresetAccountingPower));
        Assert.True(ConvertConfig.IsBuiltInPreset(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen));
    }

    [Fact]
    public void IsBuiltInPreset_ユーザープリセット名はfalse()
    {
        Assert.False(ConvertConfig.IsBuiltInPreset("ユーザー定義"));
    }

    [Fact]
    public void IsBuiltInPreset_空文字はfalse()
    {
        Assert.False(ConvertConfig.IsBuiltInPreset(""));
    }

    [Fact]
    public void BuiltInPresetAccountingPower_定数が定義されている()
    {
        Assert.Contains("全力会計", ConvertConfig.BuiltInPresetAccountingPower);
        Assert.Contains("Built-in", ConvertConfig.BuiltInPresetAccountingPower);
    }

    [Fact]
    public void BuiltInPresetAlphanumericHanKanaZen_定数が定義されている()
    {
        Assert.Contains("英数記号半角", ConvertConfig.BuiltInPresetAlphanumericHanKanaZen);
        Assert.Contains("かな全角", ConvertConfig.BuiltInPresetAlphanumericHanKanaZen);
        Assert.Contains("Built-in", ConvertConfig.BuiltInPresetAlphanumericHanKanaZen);
    }

    [Fact]
    public void LoadPreset_英数記号半角かな全角を読み込める()
    {
        var config = new ConvertConfig();
        var result = config.LoadPreset(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen);

        Assert.True(result);
        Assert.True(config.IsEnabledZenHan);
        // 英数記号 → 全て半角
        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeNumber);
        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeAlphabet);
        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeSymbolParenthesis);
        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeSymbolExclamation);
        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeSymbolSpace);
        // 半角カナ → 全角カタカナ
        Assert.Equal(ZenHanKanaMode.ToZenKata, config.ConvertModeKanaHan);
        Assert.Equal(ZenHanKanaMode.None, config.ConvertModeKanaZenKata);
        Assert.Equal(ZenHanKanaMode.None, config.ConvertModeKanaZenHira);
        // かな記号 → 半角
        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeEtcKanaVoice);
        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeEtcKanaLeftCornerBracket);
        // かな約物 → 全角
        Assert.Equal(ZenHanEtcZenHanAsciiMode.ToZen, config.ConvertModeEtcKanaProlong);
        Assert.Equal(ZenHanEtcZenHanAsciiMode.ToZen, config.ConvertModeEtcKanaPeriod);
        // バックスラッシュ/円記号 → 半角円記号
        Assert.Equal(ZenHanEtcYenMode.ToHanYen, config.ConvertModeEtcBSlashHan);
        Assert.Equal(ZenHanEtcYenMode.ToHanYen, config.ConvertModeEtcBSlashZen);
        Assert.Equal(ZenHanEtcYenMode.None, config.ConvertModeEtcYenHan);
        Assert.Equal(ZenHanEtcYenMode.ToHanYen, config.ConvertModeEtcYenZen);
        // 特殊文字: タブスペース化、改行は変換なし
        Assert.Equal(ZenHanEtcSpecial.ToHanSpace, config.ConvertModeEtcTabSpace);
        Assert.Equal(ZenHanEtcSpecial.None, config.ConvertModeEtcNewline);
        Assert.Equal(ZenHanEtcSpecial.ToHanSpace, config.ConvertModeEtcMultiSpace);
    }

    [Fact]
    public void LoadPreset_全力会計は数値と英字を半角に保つ()
    {
        // 後段の _toZenSymbolSetters ループ（その他の記号 → 全角）が数値・英字を
        // 巻き込まないことを検証します。会計帳票では数値・英字の半角が必須です。
        var config = new ConvertConfig();

        config.LoadPreset(ConvertConfig.BuiltInPresetAccountingPower);

        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeNumber);
        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeAlphabet);
        // 一部記号は半角、その他の記号は全角
        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeSymbolParenthesis);
        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeSymbolComma);
        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeSymbolPeriod);
        Assert.Equal(ZenHanMode.ToZen, config.ConvertModeSymbolExclamation);
        Assert.Equal(ZenHanMode.ToZen, config.ConvertModeSymbolTilde);
    }

    [Fact]
    public void プロパティ変更が正しく反映される()
    {
        var config = new ConvertConfig();
        config.IsEnabledZenHan = true;
        config.ConvertModeNumber = ZenHanMode.ToHan;
        config.ConvertModeAlphabet = ZenHanMode.ToZen;

        Assert.True(config.IsEnabledZenHan);
        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeNumber);
        Assert.Equal(ZenHanMode.ToZen, config.ConvertModeAlphabet);
    }

    [Fact]
    public void ReplacePairs_設定と取得()
    {
        var config = new ConvertConfig();
        var pairs = new List<ReplacePair> { new("abc", "xyz") };
        config.ReplacePairs = pairs;

        Assert.Single(config.ReplacePairs);
        Assert.Equal("abc", config.ReplacePairs[0].Search);
    }
}
