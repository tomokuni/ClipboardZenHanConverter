using EsUtil.ClipboardZenHanConverter.Core.Enums;
using EsUtil.ClipboardZenHanConverter.Core.Models;
using Xunit;

namespace EsUtil.ClipboardZenHanConverter.Tests.Core.Models;

/// <summary><see cref="SegmentItem"/> / <see cref="SegmentDefine"/> の値の保持を検証します。</summary>
public sealed class SegmentItemTests
{
    [Fact]
    public void SegmentItem_既定では有効()
    {
        var item = new SegmentItem("半角", ZenHanMode.ToHan);

        Assert.True(item.IsEnabled);
    }

    [Fact]
    public void SegmentItem_無効状態を保持する()
    {
        var item = new SegmentItem("半角", ZenHanMode.ToHan, IsEnabled: false);

        Assert.False(item.IsEnabled);
    }

    [Fact]
    public void SegmentItem_レコードの等価性()
    {
        var a = new SegmentItem("半角", ZenHanMode.ToHan);
        var b = new SegmentItem("半角", ZenHanMode.ToHan);
        var c = new SegmentItem("全角", ZenHanMode.ToHan);

        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
    }

    [Fact]
    public void SegmentDefine_既定値はセグメント未指定と自動高さ()
    {
        var def = new SegmentDefine("数字 の変換", ConvertConfig.Mode.Number);

        Assert.Equal("数字 の変換", def.Label);
        Assert.Null(def.Segments);
        Assert.Null(def.ForceEnableState);
        Assert.True(double.IsNaN(def.Height));
    }

    [Fact]
    public void SegmentDefine_セグメントと高さと有効状態を保持する()
    {
        SegmentItem[] segments = [new("なし", ZenHanMode.None), new("半角", ZenHanMode.ToHan)];

        var def = new SegmentDefine("連続スペース", ConvertConfig.Mode.EtcMultiSpace,
            Height: 80, Segments: segments, ForceEnableState: false);

        Assert.Equal(80, def.Height);
        Assert.Equal(segments, def.Segments);
        Assert.False(def.ForceEnableState);
    }

    [Fact]
    public void SegmentDefine_Modeを通じて設定を読み書きできる()
    {
        var config = TestHelper.CreateDefaultConfig();
        var def = new SegmentDefine("数字 の変換", ConvertConfig.Mode.Number);

        def.Mode.Set(config, ZenHanMode.ToHan);

        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeNumber);
        Assert.Equal(ZenHanMode.ToHan, def.Mode.Get(config));
    }
}
