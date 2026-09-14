using ClipboardZenHanConverter.Core.Models;
using Xunit;

namespace ClipboardZenHanConverter.Tests.Core.Models;

public class SegmentItemTests
{
    [Fact]
    public void SegmentItem_デフォルトIsEnabled()
    {
        var item = new SegmentItem("半角", "Value");
        Assert.True(item.IsEnabled);
        Assert.Equal("半角", item.Content);
        Assert.Equal("Value", item.Value);
    }

    [Fact]
    public void SegmentItem_IsEnabledを明示指定()
    {
        var item = new SegmentItem("無効", "Value", IsEnabled: false);
        Assert.False(item.IsEnabled);
    }

    [Fact]
    public void SegmentDefine_デフォルト値()
    {
        var def = new SegmentDefine("ラベル", ConvertConfig.Mode.Number);
        Assert.Equal("ラベル", def.Label);
        Assert.Same(ConvertConfig.Mode.Number, def.Mode);
        Assert.True(double.IsNaN(def.Height));
        Assert.Null(def.Segments);
        Assert.Null(def.ForceEnableState);
    }

    [Fact]
    public void SegmentDefine_全パラメーター指定()
    {
        var segments = new[] { new SegmentItem("A", 1) };
        var def = new SegmentDefine("ラベル", ConvertConfig.Mode.Number, Height: 50, Segments: segments, ForceEnableState: true);
        Assert.Equal(50, def.Height);
        Assert.Same(segments, def.Segments);
        Assert.True(def.ForceEnableState);
    }

    [Fact]
    public void SegmentItem_レコード等価性()
    {
        var a = new SegmentItem("半角", "ToHan");
        var b = new SegmentItem("半角", "ToHan");
        var c = new SegmentItem("全角", "ToZen");

        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
    }
}
