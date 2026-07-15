using ClipboardZenHanConverter.Core.Models;
using Xunit;

namespace ClipboardZenHanConverter.Tests.Core.Models;

public class ReplacePairTests
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

    [Fact]
    public void TryValidate_検索文字列が空()
    {
        var result = ReplacePair.TryValidate("", "xyz", false, out var error);
        Assert.False(result);
        Assert.Equal("検索文字列は必須です", error);
    }

    [Fact]
    public void TryValidate_検索文字列がnull()
    {
        var result = ReplacePair.TryValidate(null!, "xyz", false, out var error);
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

    [Fact]
    public void TryValidate_不正な正規表現()
    {
        var result = ReplacePair.TryValidate("[invalid", "x", true, out var error);
        Assert.False(result);
        Assert.Equal("正規表現の形式が正しくありません", error);
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
    public void レコードのToString()
    {
        var pair = new ReplacePair("abc", "xyz");
        Assert.Contains("abc", pair.ToString());
        Assert.Contains("xyz", pair.ToString());
    }
}
