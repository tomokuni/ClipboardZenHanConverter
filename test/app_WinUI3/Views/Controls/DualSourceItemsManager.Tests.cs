// このソースコードは、UTF-8、LF で作成します。

using EsUtil.ClipboardZenHanConverter.App.WinUI.Views.Controls;
using Xunit;

namespace EsUtil.ClipboardZenHanConverter.Tests.App.WinUI.Views.Controls;

/// <summary>DualSourceItemsManager の子要素管理状態のテスト。</summary>
/// <remarks>UI スレッドを必要としない状態管理のみを検証する。子要素の再構築は UIElementCollection を扱うため対象外とする。</remarks>
public class DualSourceItemsManagerTests
{
    [Fact]
    public void ItemsSourceFirst_既定値はtrue()
    {
        var manager = new DualSourceItemsManager();

        Assert.True(manager.ItemsSourceFirst);
    }

    [Fact]
    public void ItemsSourceFirst_設定した値が保持される()
    {
        var manager = new DualSourceItemsManager { ItemsSourceFirst = false };

        Assert.False(manager.ItemsSourceFirst);
    }

    [Fact]
    public void HasDirectChildItemsStored_退避前はfalse()
    {
        var manager = new DualSourceItemsManager();

        Assert.False(manager.HasDirectChildItemsStored);
    }

    [Fact]
    public void DirectChildItems_退避前は空()
    {
        var manager = new DualSourceItemsManager();

        Assert.Empty(manager.DirectChildItems);
    }

    [Fact]
    public void Reset_ItemsSourceFirstを既定値へ戻す()
    {
        var manager = new DualSourceItemsManager { ItemsSourceFirst = false };

        manager.Reset();

        Assert.True(manager.ItemsSourceFirst);
    }

    [Fact]
    public void Reset_退避前でも状態を保持する()
    {
        var manager = new DualSourceItemsManager();

        manager.Reset();

        Assert.False(manager.HasDirectChildItemsStored);
        Assert.Empty(manager.DirectChildItems);
    }
}
