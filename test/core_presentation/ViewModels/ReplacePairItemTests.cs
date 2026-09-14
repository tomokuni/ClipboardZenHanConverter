using ClipboardZenHanConverter.Core.Models;
using Xunit;

namespace ClipboardZenHanConverter.Tests.Presentation.ViewModels;

/// <summary><see cref="ReplacePairItem"/> の編集値とバリデーションを検証します。</summary>
public sealed class ReplacePairItemTests
{
    [Fact]
    public void 既定値は空文字と正規表現なし()
    {
        var item = new ReplacePairItem();

        Assert.Equal(string.Empty, item.Search);
        Assert.Equal(string.Empty, item.Replace);
        Assert.False(item.IsRegex);
        Assert.Null(item.ErrorMessage);
    }

    [Fact]
    public void ReplacePairからのコピーで値が設定される()
    {
        var pair = new ReplacePair("検索", "置換", true);

        var item = new ReplacePairItem(pair);

        Assert.Equal("検索", item.Search);
        Assert.Equal("置換", item.Replace);
        Assert.True(item.IsRegex);
    }

    [Fact]
    public void 検索文字列が空の場合はエラーになる()
    {
        var item = new ReplacePairItem { Search = "", Replace = "置換" };

        Assert.NotNull(item.Validate());
        Assert.NotNull(item.ErrorMessage);
    }

    [Fact]
    public void 置換文字列が空の場合はエラーになる()
    {
        var item = new ReplacePairItem { Search = "検索", Replace = "" };

        Assert.NotNull(item.Validate());
        Assert.NotNull(item.ErrorMessage);
    }

    [Fact]
    public void 有効な値の場合はエラーが消える()
    {
        var item = new ReplacePairItem { Search = "検索", Replace = "置換" };
        item.Validate();
        Assert.Null(item.ErrorMessage);

        item.Search = "";
        item.Validate();
        Assert.NotNull(item.ErrorMessage);
    }

    [Fact]
    public void 正規表現が不正な場合はエラーになる()
    {
        var item = new ReplacePairItem { Search = "(", Replace = "x", IsRegex = true };

        Assert.NotNull(item.Validate());
    }

    [Fact]
    public void ToPairは現在の値を反映する()
    {
        var item = new ReplacePairItem { Search = "検索", Replace = "置換", IsRegex = true };

        var pair = item.ToPair();

        Assert.Equal("検索", pair.Search);
        Assert.Equal("置換", pair.Replace);
        Assert.True(pair.IsRegex);
    }

    [Fact]
    public void 編集値の変更は通知される()
    {
        var item = new ReplacePairItem();
        var changed = new List<string?>();
        item.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        item.Search = "検索";

        Assert.Contains(nameof(ReplacePairItem.Search), changed);
    }
}
