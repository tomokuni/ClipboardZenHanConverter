using Xunit;
using ClipboardZenHanConverter.Core.Logic;
using ClipboardZenHanConverter.Core.Models;
using ClipboardZenHanConverter.Core.Enums;

namespace ClipboardZenHanConverter.Tests;

public sealed partial class CharConverterTests : IDisposable
{
    private readonly ConvertConfig _config;
    private readonly CharConverter _converter;

    public CharConverterTests()
    {
        _config = TestHelper.CreateDefaultConfig();
        _config.IsEnabledZenHan = true;
        _converter = new CharConverter(_config);
    }

    public void Dispose()
    {
        _converter.Dispose();
        GC.SuppressFinalize(this);
    }

    #region コンストラクタ・プロパティ

    [Fact]
    public void Constructor_Configを正しく保持する()
    {
        Assert.Same(_config, _converter.Config);
    }

    #endregion

    #region Convert - null/空文字

    [Fact]
    public void Convert_nullを渡すとnullを返す()
    {
        Assert.Null(_converter.Convert(null!));
    }

    [Fact]
    public void Convert_空文字を渡すと空文字を返す()
    {
        Assert.Equal(string.Empty, _converter.Convert(string.Empty));
    }

    #endregion

    #region Convert - 無効時

    [Fact]
    public void Convert_IsEnabledZenHanがfalseの場合は変換しない()
    {
        _config.IsEnabledZenHan = false;
        _config.ConvertModeNumber = ZenHanMode.ToHan;

        var result = _converter.Convert("１２３");

        Assert.Equal("１２３", result);
    }

    #endregion

    #region Convert - 数字変換

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

    #endregion

    #region Convert - 英字変換

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

    #endregion

    #region Convert - 記号変換

    public static TheoryData<ZenHanMode, string, string> SymbolTestData() => new()
    {
        { ZenHanMode.ToHan, "！", "!" },
        { ZenHanMode.ToZen, "!", "！" },
    };

    [Theory]
    [MemberData(nameof(SymbolTestData))]
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

    #endregion

    #region Convert - カナ変換

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

    #endregion

    #region Convert - 改行・スペース変換

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
    [InlineData(ZenHanEtcSpecial.ToHanSpace, "A\nB", "A B")]
    [InlineData(ZenHanEtcSpecial.ToHanSpace, "A\rB", "A B")]
    public void Convert_各種改行コードを正しく変換する(ZenHanEtcSpecial mode, string input, string expected)
    {
        _config.ConvertModeEtcNewline = mode;

        Assert.Equal(expected, _converter.Convert(input));
    }

    [Theory]
    [InlineData(ZenHanEtcSpecial.ToHanSpace, "A   B", "A B")]
    [InlineData(ZenHanEtcSpecial.ToHanSpace, "A  B", "A B")]
    [InlineData(ZenHanEtcSpecial.None, "A   B", "A   B")]
    public void Convert_連続スペース変換モードに応じた変換を行う(ZenHanEtcSpecial mode, string input, string expected)
    {
        _config.ConvertModeEtcMultiSpace = mode;

        Assert.Equal(expected, _converter.Convert(input));
    }

    #endregion

    #region Convert - タブ変換

    [Theory]
    [InlineData(ZenHanEtcSpecial.ToHanSpace, "A\tB", "A B")]
    [InlineData(ZenHanEtcSpecial.ToZenSpace, "A\tB", "A　B")]
    [InlineData(ZenHanEtcSpecial.None, "A\tB", "A\tB")]
    public void Convert_タブ変換モードに応じた変換を行う(ZenHanEtcSpecial mode, string input, string expected)
    {
        _config.ConvertModeEtcTabSpace = mode;

        Assert.Equal(expected, _converter.Convert(input));
    }

    #endregion

    #region GetConvertPairs - キャッシュ

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

    #endregion

    #region Dispose

    [Fact]
    public void Dispose_複数回呼び出しても例外を送出しない()
    {
        var config = TestHelper.CreateDefaultConfig();
        var converter = new CharConverter(config);

        converter.Dispose();

        var ex = Record.Exception(() => converter.Dispose());

        Assert.Null(ex);
    }

    [Fact]
    public void Dispose_PropertyChanged購読が解除される()
    {
        var config = TestHelper.CreateDefaultConfig();
        config.IsEnabledZenHan = true;
        var converter = new CharConverter(config);
        var pairs1 = converter.GetConvertPairs();

        converter.Dispose();
        config.ConvertModeNumber = ZenHanMode.ToHan;
        var pairs2 = converter.GetConvertPairs();

        Assert.Same(pairs1, pairs2);
    }

    #endregion

    #region 複合テスト

    [Fact]
    public void Convert_複数の変換設定を同時に適用する()
    {
        _config.ConvertModeNumber = ZenHanMode.ToHan;
        _config.ConvertModeAlphabet = ZenHanMode.ToHan;

        var result = _converter.Convert("１２３ＡＢＣ");

        Assert.Equal("123ABC", result);
    }

    [Fact]
    public void Convert_変換対象外の文字は変更しない()
    {
        _config.ConvertModeNumber = ZenHanMode.ToHan;

        var result = _converter.Convert("漢字とひらがな１２３");

        Assert.Equal("漢字とひらがな123", result);
    }

    #endregion
}

