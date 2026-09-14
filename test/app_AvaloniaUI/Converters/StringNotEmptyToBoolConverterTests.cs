using ClipboardZenHanConverter.App.AvaloniaUI.Converters;
using System.Globalization;
using Xunit;

namespace ClipboardZenHanConverter.Tests.App.AvaloniaUI.Converters;

/// <summary><see cref="StringNotEmptyToBoolConverter"/> の変換を検証します。</summary>
public sealed class StringNotEmptyToBoolConverterTests
{
    /// <summary>検証対象のコンバーター。</summary>
    private readonly StringNotEmptyToBoolConverter _converter = StringNotEmptyToBoolConverter.Instance;

    [Fact]
    public void Instanceは共有インスタンスを返す()
    {
        Assert.Same(StringNotEmptyToBoolConverter.Instance, StringNotEmptyToBoolConverter.Instance);
    }

    [Theory]
    [InlineData("abc", true)]
    [InlineData(" ", true)]
    [InlineData("", false)]
    public void Convert_空文字のみfalseを返す(string value, bool expected)
        => Assert.Equal(expected, _converter.Convert(value, typeof(bool), null, CultureInfo.InvariantCulture));

    [Fact]
    public void Convert_nullはfalseを返す()
        => Assert.Equal(false, _converter.Convert(null, typeof(bool), null, CultureInfo.InvariantCulture));

    [Fact]
    public void Convert_文字列以外はfalseを返す()
        => Assert.Equal(false, _converter.Convert(123, typeof(bool), null, CultureInfo.InvariantCulture));

    [Fact]
    public void ConvertBack_サポートしない()
        => Assert.Throws<NotSupportedException>(
            () => _converter.ConvertBack(true, typeof(string), null, CultureInfo.InvariantCulture));
}
