using EsUtil.ClipboardZenHanConverter.Core.Enums;
using EsUtil.ClipboardZenHanConverter.Core.Models;
using System;
using System.IO;
using Xunit;

namespace EsUtil.ClipboardZenHanConverter.Tests.Core.Models;

/// <summary><see cref="ConvertConfig"/> の設定・プリセット・入出力を検証します。</summary>
/// <remarks>ファイルを書き込むテストでは、実ユーザーの設定ファイルを汚さないよう<br/>
/// <see cref="SettingsPersistenceBase{TSettings}.AutoSaveFileName"/> を一時ディレクトリへ差し替えます。</remarks>
public sealed class ConvertConfigTests : IDisposable
{
    private readonly string _tempDirectory = TestHelper.CreateTempDirectory();

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, recursive: true);

        GC.SuppressFinalize(this);
    }

    /// <summary>書き込みを一時ディレクトリへ逃がした設定を作成します。</summary>
    /// <param name="fileName">自動保存先のファイル名。</param>
    /// <returns>自動保存先を差し替えた ConvertConfig。</returns>
    private ConvertConfig CreateConfig(string fileName = "Settings.json")
    {
        var config = TestHelper.CreateDefaultConfig();
        config.AutoSaveFileName = Path.Combine(_tempDirectory, fileName);
        return config;
    }

    [Fact]
    public void Constructor_既定値は全てNoneで置換ルールは空()
    {
        var config = TestHelper.CreateDefaultConfig();

        Assert.False(config.IsEnabledZenHan);
        Assert.Equal(ZenHanMode.None, config.ConvertModeNumber);
        Assert.Equal(ZenHanMode.None, config.ConvertModeAlphabet);
        Assert.Equal(ZenHanKanaMode.None, config.ConvertModeKanaHan);
        Assert.Equal(ZenHanEtcSpecial.None, config.ConvertModeEtcMultiSpace);
        Assert.Empty(config.ReplacePairs);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Mode_全角半角変換の有効フラグのGetとSetが往復する(bool value)
    {
        var config = TestHelper.CreateDefaultConfig();

        ConvertConfig.Mode.IsEnabledZenHan.Set(config, value);

        Assert.Equal(value, config.IsEnabledZenHan);
        Assert.Equal(value, ConvertConfig.Mode.IsEnabledZenHan.Get(config));
    }

    [Fact]
    public void Mode_その他の記号は全角設定の対象になる()
    {
        Assert.Equal(ZenHanMode.ToZen, ConvertConfig.Mode.SymbolExclamation.ToZenValue);
        Assert.Equal(ZenHanMode.ToZen, ConvertConfig.Mode.SymbolTilde.ToZenValue);
    }

    [Fact]
    public void Mode_全角設定の対象外はToZenValueがnullになる()
    {
        Assert.Null(ConvertConfig.Mode.Number.ToZenValue);
        Assert.Null(ConvertConfig.Mode.Alphabet.ToZenValue);
        Assert.Null(ConvertConfig.Mode.SymbolParenthesis.ToZenValue);
        Assert.Null(ConvertConfig.Mode.SymbolComma.ToZenValue);
    }

    [Theory]
    [InlineData(ZenHanMode.ToZen)]
    [InlineData(ZenHanMode.ToHan)]
    [InlineData(ZenHanMode.None)]
    public void Mode_数値モードのGetとSetが往復する(ZenHanMode value)
    {
        var config = TestHelper.CreateDefaultConfig();

        ConvertConfig.Mode.Number.Set(config, value);

        Assert.Equal(value, config.ConvertModeNumber);
        Assert.Equal(value, ConvertConfig.Mode.Number.Get(config));
    }

    [Fact]
    public void Mode_特殊文字モードのGetとSetが往復する()
    {
        var config = TestHelper.CreateDefaultConfig();

        ConvertConfig.Mode.EtcMultiSpace.Set(config, ZenHanEtcSpecial.ToHanSpace);

        Assert.Equal(ZenHanEtcSpecial.ToHanSpace, config.ConvertModeEtcMultiSpace);
        Assert.Equal(ZenHanEtcSpecial.ToHanSpace, ConvertConfig.Mode.EtcMultiSpace.Get(config));
    }

    [Fact]
    public void ExportToFile_ファイルが生成される()
    {
        var config = CreateConfig();
        config.ConvertModeNumber = ZenHanMode.ToHan;
        var file = Path.Combine(_tempDirectory, "export.json");

        config.ExportToFile(file);

        Assert.True(File.Exists(file));
        Assert.Contains("ConvertModeNumber", File.ReadAllText(file));
    }

    [Fact]
    public void ImportFromFile_全設定と置換ルールが反映される()
    {
        var source = CreateConfig("source.json");
        source.ConvertModeNumber = ZenHanMode.ToZen;
        source.ConvertModeKanaHan = ZenHanKanaMode.ToZenKata;
        source.ReplacePairs = [new ReplacePair("aaa", "bbb")];
        var file = Path.Combine(_tempDirectory, "import-source.json");
        source.ExportToFile(file);

        var target = CreateConfig("target.json");
        var result = target.ImportFromFile(file);

        Assert.True(result);
        Assert.Equal(ZenHanMode.ToZen, target.ConvertModeNumber);
        Assert.Equal(ZenHanKanaMode.ToZenKata, target.ConvertModeKanaHan);
        Assert.Equal(source.ReplacePairs, target.ReplacePairs);
    }

    [Fact]
    public void ImportFromFile_置換ルールは複製される()
    {
        var source = CreateConfig("source.json");
        source.ReplacePairs = [new ReplacePair("aaa", "bbb")];
        var file = Path.Combine(_tempDirectory, "copy-source.json");
        source.ExportToFile(file);

        var target = CreateConfig("target.json");
        target.ImportFromFile(file);

        Assert.NotSame(source.ReplacePairs, target.ReplacePairs);
    }

    [Fact]
    public void ImportFromFile_存在しないファイルはfalseを返す()
    {
        var config = CreateConfig();

        var result = config.ImportFromFile(Path.Combine(_tempDirectory, "not-found.json"));

        Assert.False(result);
    }

    [Fact]
    public void ImportFromFile_不正なJSONはfalseを返し設定を維持する()
    {
        var config = CreateConfig();
        config.ConvertModeNumber = ZenHanMode.ToHan;
        var file = Path.Combine(_tempDirectory, "broken.json");
        File.WriteAllText(file, "{ this is not json");

        var result = config.ImportFromFile(file);

        Assert.False(result);
        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeNumber);
    }

    [Fact]
    public void IsBuiltInPreset_組み込みプリセットを判定する()
    {
        Assert.True(ConvertConfig.IsBuiltInPreset(ConvertConfig.BuiltInPresetAccountingPower));
        Assert.True(ConvertConfig.IsBuiltInPreset(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen));
        Assert.False(ConvertConfig.IsBuiltInPreset("存在しないプリセット"));
    }

    [Fact]
    public void GetPresetNames_組み込みプリセットを含む()
    {
        var names = ConvertConfig.GetPresetNames();

        Assert.Contains(ConvertConfig.BuiltInPresetAccountingPower, names);
        Assert.Contains(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen, names);
    }

    [Fact]
    public void LoadPreset_英数記号半角かな全角プリセットを適用する()
    {
        var config = TestHelper.CreateDefaultConfig();

        var result = config.LoadPreset(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen);

        Assert.True(result);
        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeNumber);
        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeAlphabet);
        Assert.Equal(ZenHanKanaMode.ToZenKata, config.ConvertModeKanaHan);
        Assert.Equal(ZenHanEtcSpecial.None, config.ConvertModeEtcNewline);
    }

    [Fact]
    public void LoadPreset_全力会計プリセットを適用する()
    {
        var config = TestHelper.CreateDefaultConfig();

        var result = config.LoadPreset(ConvertConfig.BuiltInPresetAccountingPower);

        Assert.True(result);
        Assert.Equal(ZenHanMode.ToZen, config.ConvertModeSymbolExclamation);
        Assert.Equal(ZenHanKanaMode.ToZenKata, config.ConvertModeKanaHan);
        Assert.Equal(ZenHanEtcSpecial.ToHanSpace, config.ConvertModeEtcNewline);
        Assert.Equal(ZenHanEtcZenHanAsciiMode.ToAscii, config.ConvertModeEtcKanaProlong);
        Assert.Empty(config.ReplacePairs);
    }

    [Fact]
    public void LoadPreset_全力会計は数値と英字を半角に保つ()
    {
        // 後段の _toZenSymbolSetters ループ（その他の記号 → 全角）が数値・英字を
        // 巻き込まないことを検証します。会計帳票では数値・英字の半角が必須です。
        var config = TestHelper.CreateDefaultConfig();

        config.LoadPreset(ConvertConfig.BuiltInPresetAccountingPower);

        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeNumber);
        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeAlphabet);
    }

    [Fact]
    public void LoadPreset_全力会計は一部記号とその他の記号を正しく振り分ける()
    {
        var config = TestHelper.CreateDefaultConfig();

        config.LoadPreset(ConvertConfig.BuiltInPresetAccountingPower);

        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeSymbolParenthesis);
        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeSymbolComma);
        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeSymbolPeriod);
        Assert.Equal(ZenHanMode.ToZen, config.ConvertModeSymbolExclamation);
        Assert.Equal(ZenHanMode.ToZen, config.ConvertModeSymbolTilde);
    }

    [Theory]
    [InlineData(ConvertConfig.BuiltInPresetAccountingPower)]
    [InlineData(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen)]
    public void LoadPreset_組み込みプリセットは全角半角変換を有効にする(string presetName)
    {
        var config = TestHelper.CreateDefaultConfig();

        config.LoadPreset(presetName);

        Assert.True(config.IsEnabledZenHan);
    }

    [Fact]
    public void LoadPreset_存在しないプリセットはfalseを返す()
    {
        var config = TestHelper.CreateDefaultConfig();

        var result = config.LoadPreset("存在しないプリセット");

        Assert.False(result);
    }

    [Fact]
    public void LoadPreset_空文字はfalseを返す()
    {
        var config = TestHelper.CreateDefaultConfig();

        Assert.False(config.LoadPreset(string.Empty));
        Assert.False(config.LoadPreset("   "));
    }

    [Fact]
    public void DeletePreset_組み込みプリセットは削除できない()
    {
        var ex = Record.Exception(() => ConvertConfig.DeletePreset(ConvertConfig.BuiltInPresetAccountingPower));

        Assert.Null(ex);
        Assert.True(ConvertConfig.IsBuiltInPreset(ConvertConfig.BuiltInPresetAccountingPower));
    }

    [Fact]
    public void SavePreset_空文字は何もしない()
    {
        var config = TestHelper.CreateDefaultConfig();
        var before = ConvertConfig.GetPresetNames();

        var ex = Record.Exception(() => config.SavePreset("   "));

        Assert.Null(ex);
        Assert.Equal(before, ConvertConfig.GetPresetNames());
    }

    [Fact]
    public void FindMatchingPreset_組み込みプリセット適用後に一致する名前を返す()
    {
        var config = TestHelper.CreateDefaultConfig();
        config.LoadPreset(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen);

        Assert.Equal(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen, config.FindMatchingPreset());
    }
}
