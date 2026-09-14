using ClipboardZenHanConverter.Core.Geometry;
using Xunit;

namespace ClipboardZenHanConverter.Tests.Core.Geometry;

/// <summary><see cref="PointD"/> の値の保持と等価性を検証します。</summary>
public sealed class PointDTests
{
    [Fact]
    public void Constructor_座標を保持する()
    {
        var point = new PointD(100.5, -200.25);

        Assert.Equal(100.5, point.X);
        Assert.Equal(-200.25, point.Y);
    }

    [Fact]
    public void 同じ座標の値は等価と判定する()
    {
        Assert.Equal(new PointD(1.5, 2.5), new PointD(1.5, 2.5));
        Assert.NotEqual(new PointD(1.5, 2.5), new PointD(1.5, 3.5));
    }
}
