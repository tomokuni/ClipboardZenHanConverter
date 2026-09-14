using EsUtil.ClipboardZenHanConverter.Core.Models;
using Xunit;

namespace EsUtil.ClipboardZenHanConverter.Tests.Core.Models;

/// <summary><see cref="ReplacePair"/> の検証ロジックと値の等価性を検証します。</summary>
public sealed class ReplacePairTests
{
    [Fact]
    public void TryValidate_有効な通常文字列()
    {
        var result = ReplacePair.TryValidate("abc", "xyz", false, out var error);

        Assert.True(result);
        Assert.Null(error);
    }

    [Fact]
    public void TryValidate_有効な正規表現()
    {
        var result = ReplacePair.TryValidate(@"\d+", "NUM", true, out var error);

        Assert.True(result);
        Assert.Null(error);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void TryValidate_検索文字列が空またはnull(string? search)
    {
        var result = ReplacePair.TryValidate(search!, "xyz", false, out var error);

        Assert.False(result);
        Assert.Equal("検索文字列は必須です", error);
    }

    [Fact]
    public void TryValidate_置換文字列が空()
    {
        var result = ReplacePair.TryValidate("abc", "", false, out var error);

        Assert.False(result);
        Assert.Equal("置換文字列は必須です", error);
    }

    [Theory]
    [InlineData("[invalid")]
    [InlineData("(unclosed")]
    [InlineData("a{2,1}")]
    public void TryValidate_不正な正規表現(string pattern)
    {
        var result = ReplacePair.TryValidate(pattern, "x", true, out var error);

        Assert.False(result);
        Assert.Equal("正規表現の形式が正しくありません", error);
    }

    [Fact]
    public void TryValidate_正規表現フラグ無効時はパターンを検証しない()
    {
        var result = ReplacePair.TryValidate("[invalid", "x", false, out var error);

        Assert.True(result);
        Assert.Null(error);
    }

    [Fact]
    public void レコードの等価性()
    {
        var a = new ReplacePair("abc", "xyz", true);
        var b = new ReplacePair("abc", "xyz", true);
        var c = new ReplacePair("abc", "xyz", false);

        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
    }

    [Fact]
    public void IsRegexの既定値はfalse()
    {
        Assert.False(new ReplacePair("abc", "xyz").IsRegex);
    }

    [Fact]
    public void レコードのToString()
    {
        var pair = new ReplacePair("abc", "xyz");

        Assert.Contains("abc", pair.ToString());
        Assert.Contains("xyz", pair.ToString());
    }
}
