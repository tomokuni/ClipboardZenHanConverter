using ClipboardZenHanConverter.Core.Geometry;
using Xunit;

namespace ClipboardZenHanConverter.Tests.Core.Geometry;

/// <summary><see cref="RectD"/> の値の保持と端座標の算出を検証します。</summary>
public sealed class RectDTests
{
    [Fact]
    public void Constructor_位置と寸法を保持する()
    {
        var rect = new RectD(10.5, 20.25, 300.5, 400.75);

        Assert.Equal(10.5, rect.X);
        Assert.Equal(20.25, rect.Y);
        Assert.Equal(300.5, rect.Width);
        Assert.Equal(400.75, rect.Height);
    }

    [Fact]
    public void 端座標は位置と寸法から算出される()
    {
        var rect = new RectD(-1920, -100, 3840, 1300);

        Assert.Equal(-1920, rect.Left);
        Assert.Equal(1920, rect.Right);
        Assert.Equal(-100, rect.Top);
        Assert.Equal(1200, rect.Bottom);
    }

    [Fact]
    public void 同じ位置と寸法の値は等価と判定する()
    {
        Assert.Equal(new RectD(0, 0, 800, 600), new RectD(0, 0, 800, 600));
        Assert.NotEqual(new RectD(0, 0, 800, 600), new RectD(0, 0, 800, 601));
    }
}
