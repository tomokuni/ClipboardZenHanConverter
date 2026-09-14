using ClipboardZenHanConverter.Core.Enums;
using ClipboardZenHanConverter.Core.Logic;
using ClipboardZenHanConverter.Core.Models;
using Xunit;

namespace ClipboardZenHanConverter.Tests.Core.Logic;

/// <summary><see cref="CharConverter"/> の変換動作を検証します。</summary>
public sealed class CharConverterTests : IDisposable
{
    private readonly ConvertConfig _config;
    private readonly CharConverter _converter;

    public CharConverterTests()
    {
        _config = TestHelper.CreateDefaultConfig();
        _converter = new CharConverter(_config);
    }

    public void Dispose()
    {
        _converter.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Constructor_Configを正しく保持する()
    {
        Assert.Same(_config, _converter.Config);
    }

    [Fact]
    public void Convert_nullを渡すとnullを返す()
    {
        Assert.Null(_converter.Convert(null!));
    }

    [Theory]
    [InlineData(ConvertConfig.BuiltInPresetAccountingPower, "２０２４年１２月３１日（金）", "2024年12月31日(金)")]
    [InlineData(ConvertConfig.BuiltInPresetAccountingPower, "（株）ＡＢＣ商事 １，２３４，５６７円", "(株)ABC商事 1,234,567円")]
    [InlineData(ConvertConfig.BuiltInPresetAccountingPower, "〒１００−０００１ 東京ﾄｳｷｮｳ", "〒100-0001 東京トウキョウ")]
    [InlineData(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen, "（株）ＡＢＣ商事「設定」２０２４年", "(株)ABC商事｢設定｣2024年")]
    [InlineData(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen, "ＴＥＬ：０３−１２３４−５６７８（代表）", "TEL:03-1234-5678(代表)")]
    [InlineData(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen, "当社の製品「Ｗｉｄｇｅｔ」は￥１，２００です。", "当社の製品｢Widget｣は¥1,200です。")]
    [InlineData(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen, "お問合せは「ｓｕｐｐｏｒｔ＠ｅｘａｍｐｌｅ．ｃｏｍ」迄", "お問合せは｢support@example.com｣迄")]
    [InlineData(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen, "ﾀﾞｲｽｷﾅﾗｰﾒﾝ! ﾏｲｳｪｲ!", "ダイスキナラーメン! マイウェイ!")]
    public void Convert_組み込みプリセットの代表的な変換結果(string presetName, string input, string expected)
    {
        // 両 UI（app_MewUI / app_WinUI3）が共有する変換仕様の正を固定します。
        // このテストが通る限り、2 つのアプリの変換結果は一致します。
        _config.LoadPreset(presetName);

        Assert.Equal(expected, _converter.Convert(input));
    }

    [Fact]
    public void Convert_空文字を渡すと空文字を返す()
    {
        Assert.Equal(string.Empty, _converter.Convert(string.Empty));
    }

    [Fact]
    public void Convert_全角半角変換の有効フラグに関わらず変換する()
    {
        // 変換の実行可否は呼び出し側（画面側）のポリシーであり、変換エンジンは判断しません。
        _config.IsEnabledZenHan = false;
        _config.ConvertModeAlphabet = ZenHanMode.ToHan;

        Assert.Equal("ABC", _converter.Convert("ＡＢＣ"));
    }

    [Theory]
    [InlineData(ZenHanMode.ToHan, "１２３", "123")]
    [InlineData(ZenHanMode.ToZen, "123", "１２３")]
    [InlineData(ZenHanMode.None, "123", "123")]
    [InlineData(ZenHanMode.None, "１２３", "１２３")]
    public void Convert_数字変換モードに応じた変換を行う(ZenHanMode mode, string input, string expected)
    {
        _config.ConvertModeNumber = mode;

        Assert.Equal(expected, _converter.Convert(input));
    }

    [Fact]
    public void Convert_数字の境界値を正しく変換する()
    {
        _config.ConvertModeNumber = ZenHanMode.ToHan;

        Assert.Equal("0123456789", _converter.Convert("０１２３４５６７８９"));
    }

    [Theory]
    [InlineData(ZenHanMode.ToHan, "ＡＢＣ", "ABC")]
    [InlineData(ZenHanMode.ToZen, "ABC", "ＡＢＣ")]
    [InlineData(ZenHanMode.ToHan, "ａｂｃ", "abc")]
    [InlineData(ZenHanMode.ToZen, "abc", "ａｂｃ")]
    public void Convert_英字変換モードに応じた変換を行う(ZenHanMode mode, string input, string expected)
    {
        _config.ConvertModeAlphabet = mode;

        Assert.Equal(expected, _converter.Convert(input));
    }

    [Theory]
    [InlineData(ZenHanMode.ToHan, "！", "!")]
    [InlineData(ZenHanMode.ToZen, "!", "！")]
    public void Convert_感嘆符変換モードに応じた変換を行う(ZenHanMode mode, string input, string expected)
    {
        _config.ConvertModeSymbolExclamation = mode;

        Assert.Equal(expected, _converter.Convert(input));
    }

    [Theory]
    [InlineData(ZenHanMode.ToHan, "（）", "()")]
    [InlineData(ZenHanMode.ToZen, "()", "（）")]
    public void Convert_括弧変換モードに応じた変換を行う(ZenHanMode mode, string input, string expected)
    {
        _config.ConvertModeSymbolParenthesis = mode;

        Assert.Equal(expected, _converter.Convert(input));
    }

    [Theory]
    [InlineData(ZenHanKanaMode.ToZenKata, "ｱｲｳｴｵ", "アイウエオ")]
    [InlineData(ZenHanKanaMode.ToZenHira, "ｱｲｳｴｵ", "あいうえお")]
    public void Convert_半角カナ変換モードに応じた変換を行う(ZenHanKanaMode mode, string input, string expected)
    {
        _config.ConvertModeKanaHan = mode;

        Assert.Equal(expected, _converter.Convert(input));
    }

    [Theory]
    [InlineData(ZenHanKanaMode.ToHan, "アイウエオ", "ｱｲｳｴｵ")]
    [InlineData(ZenHanKanaMode.ToZenHira, "アイウエオ", "あいうえお")]
    public void Convert_全角カタカナ変換モードに応じた変換を行う(ZenHanKanaMode mode, string input, string expected)
    {
        _config.ConvertModeKanaZenKata = mode;

        Assert.Equal(expected, _converter.Convert(input));
    }

    [Theory]
    [InlineData(ZenHanKanaMode.ToHan, "あいうえお", "ｱｲｳｴｵ")]
    [InlineData(ZenHanKanaMode.ToZenHira, "あいうえお", "アイウエオ")]
    public void Convert_全角ひらがな変換モードに応じた変換を行う(ZenHanKanaMode mode, string input, string expected)
    {
        _config.ConvertModeKanaZenHira = mode;

        Assert.Equal(expected, _converter.Convert(input));
    }

    [Theory]
    [InlineData(ZenHanEtcSpecial.ToHanSpace, "A\r\nB", "A B")]
    [InlineData(ZenHanEtcSpecial.ToZenSpace, "A\r\nB", "A　B")]
    [InlineData(ZenHanEtcSpecial.Remove, "A\r\nB", "AB")]
    [InlineData(ZenHanEtcSpecial.None, "A\r\nB", "A\r\nB")]
    public void Convert_改行変換モードに応じた変換を行う(ZenHanEtcSpecial mode, string input, string expected)
    {
        _config.ConvertModeEtcNewline = mode;

        Assert.Equal(expected, _converter.Convert(input));
    }

    [Theory]
    [InlineData("A\nB", "A B")]
    [InlineData("A\rB", "A B")]
    public void Convert_各種改行コードを正しく変換する(string input, string expected)
    {
        _config.ConvertModeEtcNewline = ZenHanEtcSpecial.ToHanSpace;

        Assert.Equal(expected, _converter.Convert(input));
    }

    [Theory]
    [InlineData(ZenHanEtcSpecial.ToHanSpace, "A   B", "A B")]
    [InlineData(ZenHanEtcSpecial.ToHanSpace, "A  B", "A B")]
    [InlineData(ZenHanEtcSpecial.ToZenSpace, "A   B", "A　B")]
    [InlineData(ZenHanEtcSpecial.None, "A   B", "A   B")]
    public void Convert_連続スペース変換モードに応じた変換を行う(ZenHanEtcSpecial mode, string input, string expected)
    {
        _config.ConvertModeEtcMultiSpace = mode;

        Assert.Equal(expected, _converter.Convert(input));
    }

    [Theory]
    [InlineData(ZenHanEtcSpecial.ToHanSpace, "A\tB", "A B")]
    [InlineData(ZenHanEtcSpecial.ToZenSpace, "A\tB", "A　B")]
    [InlineData(ZenHanEtcSpecial.Remove, "A\tB", "AB")]
    [InlineData(ZenHanEtcSpecial.None, "A\tB", "A\tB")]
    public void Convert_タブ変換モードに応じた変換を行う(ZenHanEtcSpecial mode, string input, string expected)
    {
        _config.ConvertModeEtcTabSpace = mode;

        Assert.Equal(expected, _converter.Convert(input));
    }

    [Theory]
    [InlineData(ZenHanEtcYenMode.ToHanBSlash, "\\", "\\")]
    [InlineData(ZenHanEtcYenMode.ToZenBSlash, "\\", "＼")]
    [InlineData(ZenHanEtcYenMode.ToHanYen, "\\", "¥")]
    [InlineData(ZenHanEtcYenMode.ToZenYen, "\\", "￥")]
    public void Convert_バックスラッシュ変換モードに応じた変換を行う(ZenHanEtcYenMode mode, string input, string expected)
    {
        _config.ConvertModeEtcBSlashHan = mode;

        Assert.Equal(expected, _converter.Convert(input));
    }

    [Theory]
    [InlineData(ZenHanEtcYenMode.ToHanBSlash, "＼", "\\")]
    [InlineData(ZenHanEtcYenMode.ToZenBSlash, "＼", "＼")]
    [InlineData(ZenHanEtcYenMode.ToHanYen, "＼", "¥")]
    [InlineData(ZenHanEtcYenMode.ToZenYen, "＼", "￥")]
    public void Convert_全角バックスラッシュ変換モードに応じた変換を行う(ZenHanEtcYenMode mode, string input, string expected)
    {
        _config.ConvertModeEtcBSlashZen = mode;

        Assert.Equal(expected, _converter.Convert(input));
    }

    [Theory]
    [InlineData(ZenHanEtcZenHanAsciiMode.ToHan, "ー", "ｰ")]
    [InlineData(ZenHanEtcZenHanAsciiMode.ToZen, "ｰ", "ー")]
    [InlineData(ZenHanEtcZenHanAsciiMode.ToAscii, "ー", "-")]
    public void Convert_長音記号変換モードに応じた変換を行う(ZenHanEtcZenHanAsciiMode mode, string input, string expected)
    {
        _config.ConvertModeEtcKanaProlong = mode;

        Assert.Equal(expected, _converter.Convert(input));
    }

    [Fact]
    public void Convert_複数の変換設定を同時に適用する()
    {
        _config.ConvertModeNumber = ZenHanMode.ToHan;
        _config.ConvertModeAlphabet = ZenHanMode.ToHan;

        Assert.Equal("123ABC", _converter.Convert("１２３ＡＢＣ"));
    }

    [Fact]
    public void Convert_変換対象外の文字は変更しない()
    {
        _config.ConvertModeNumber = ZenHanMode.ToHan;

        Assert.Equal("漢字とひらがな123", _converter.Convert("漢字とひらがな１２３"));
    }

    [Fact]
    public void Convert_ユーザー定義の通常置換を適用する()
    {
        _config.ReplacePairs = [new ReplacePair("aaa", "bbb")];

        Assert.Equal("bbb", _converter.Convert("aaa"));
    }

    [Fact]
    public void Convert_ユーザー定義の正規表現置換を適用する()
    {
        _config.ReplacePairs = [new ReplacePair(@"\d+", "NUM", IsRegex: true)];

        Assert.Equal("NUM", _converter.Convert("123"));
    }

    [Fact]
    public void Convert_検索文字列が空の置換ルールはスキップする()
    {
        _config.ReplacePairs = [new ReplacePair("", "bbb")];

        Assert.Equal("aaa", _converter.Convert("aaa"));
    }

    [Fact]
    public void Convert_不正な正規表現の置換ルールは無視して処理を継続する()
    {
        _config.ReplacePairs = [new ReplacePair("[invalid", "bbb", IsRegex: true)];

        var ex = Record.Exception(() => _converter.Convert("aaa"));

        Assert.Null(ex);
        Assert.Equal("aaa", _converter.Convert("aaa"));
    }

    [Fact]
    public void Convert_置換ルールは定義順に逐次適用する()
    {
        _config.ReplacePairs =
        [
            new ReplacePair("aaa", "bbb"),
            new ReplacePair("bbb", "ccc"),
        ];

        Assert.Equal("ccc", _converter.Convert("aaa"));
    }

    [Fact]
    public void GetConvertPairs_同一設定では同じインスタンスを返す()
    {
        var pairs1 = _converter.GetConvertPairs();
        var pairs2 = _converter.GetConvertPairs();

        Assert.Same(pairs1, pairs2);
    }

    [Fact]
    public void GetConvertPairs_設定変更後はキャッシュが無効化される()
    {
        var pairs1 = _converter.GetConvertPairs();

        _config.ConvertModeNumber = ZenHanMode.ToHan;
        var pairs2 = _converter.GetConvertPairs();

        Assert.NotSame(pairs1, pairs2);
    }

    [Fact]
    public void Dispose_複数回呼び出しても例外を送出しない()
    {
        var converter = new CharConverter(TestHelper.CreateDefaultConfig());
        converter.Dispose();

        var ex = Record.Exception(() => converter.Dispose());

        Assert.Null(ex);
    }

    [Fact]
    public void Dispose_PropertyChanged購読が解除される()
    {
        var config = TestHelper.CreateDefaultConfig();
        var converter = new CharConverter(config);
        var pairs1 = converter.GetConvertPairs();

        converter.Dispose();
        config.ConvertModeNumber = ZenHanMode.ToHan;
        var pairs2 = converter.GetConvertPairs();

        Assert.Same(pairs1, pairs2);
    }
}
