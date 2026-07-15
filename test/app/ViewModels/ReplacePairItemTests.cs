using ClipboardZenHanConverter.App.ViewModels;
using Xunit;

namespace ClipboardZenHanConverter.Tests.App.ViewModels;

public class ReplacePairItemTests
{
    [Fact]
    public void Constructor_デフォルト値()
    {
        var item = new ReplacePairItem();
        Assert.Equal(string.Empty, item.Search);
        Assert.Equal(string.Empty, item.Replace);
        Assert.False(item.IsRegex);
    }

    [Fact]
    public void プロパティ設定()
    {
        var item = new ReplacePairItem { Search = "abc", Replace = "xyz", IsRegex = true };
        Assert.Equal("abc", item.Search);
        Assert.Equal("xyz", item.Replace);
        Assert.True(item.IsRegex);
    }

    [Fact]
    public void Validate_有効な項目()
    {
        var item = new ReplacePairItem { Search = "abc", Replace = "xyz" };
        var result = item.Validate();
        Assert.Null(result);
    }

    [Fact]
    public void Validate_検索文字列が空()
    {
        var item = new ReplacePairItem { Replace = "xyz" };
        var result = item.Validate();
        Assert.NotNull(result);
    }

    [Fact]
    public void Validate_置換文字列が空()
    {
        var item = new ReplacePairItem { Search = "abc" };
        var result = item.Validate();
        Assert.NotNull(result);
    }

    [Fact]
    public void Validate_不正な正規表現()
    {
        var item = new ReplacePairItem { Search = "[invalid", Replace = "x", IsRegex = true };
        var result = item.Validate();
        Assert.NotNull(result);
    }

    [Fact]
    public void ToPair_正常変換()
    {
        var item = new ReplacePairItem { Search = "abc", Replace = "xyz", IsRegex = true };
        var pair = item.ToPair();
        Assert.Equal("abc", pair.Search);
        Assert.Equal("xyz", pair.Replace);
        Assert.True(pair.IsRegex);
    }
}
