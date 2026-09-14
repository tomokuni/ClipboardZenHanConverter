using ClipboardZenHanConverter.Core.Geometry;
using Xunit;

namespace ClipboardZenHanConverter.Tests.Core.Geometry;

/// <summary><see cref="SizeD"/> の値の保持と等価性を検証します。</summary>
public sealed class SizeDTests
{
    [Fact]
    public void Constructor_寸法を保持する()
    {
        var size = new SizeD(800.5, 600.25);

        Assert.Equal(800.5, size.Width);
        Assert.Equal(600.25, size.Height);
    }

    [Fact]
    public void 同じ寸法の値は等価と判定する()
    {
        Assert.Equal(new SizeD(800, 600), new SizeD(800, 600));
        Assert.NotEqual(new SizeD(800, 600), new SizeD(800, 601));
    }
}
