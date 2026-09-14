using EsUtil.ClipboardZenHanConverter.Core.Geometry;
using EsUtil.ClipboardZenHanConverter.Core.Helpers;
using Xunit;

namespace EsUtil.ClipboardZenHanConverter.Tests.Core.Helpers;

/// <summary><see cref="WindowPlacement"/> の表示領域内への補正ロジックを検証します。</summary>
public sealed class WindowPlacementTests
{
    /// <summary>プライマリモニタのみの表示領域（1920x1200）。</summary>
    private static readonly RectD PrimaryArea = new(0, 0, 1920, 1200);

    /// <summary>全モニタの外接矩形（左側に負座標のモニタがある構成）。</summary>
    private static readonly RectD MultiMonitorArea = new(-1920, 0, 3840, 1200);

    /// <summary>補正対象のウィンドウサイズ（800x600）。</summary>
    private static readonly SizeD DefaultWindowSize = new(800, 600);

    [Fact]
    public void ClampToVisibleArea_領域内の位置はそのまま返す()
    {
        var result = WindowPlacement.ClampToVisibleArea(new PointD(100, 200), DefaultWindowSize, PrimaryArea);

        Assert.Equal(new PointD(100, 200), result);
    }

    [Fact]
    public void ClampToVisibleArea_右にはみ出す場合は右端へ詰める()
    {
        var result = WindowPlacement.ClampToVisibleArea(new PointD(1500, 100), DefaultWindowSize, PrimaryArea);

        Assert.Equal(new PointD(1120, 100), result);
    }

    [Fact]
    public void ClampToVisibleArea_下にはみ出す場合は下端へ詰める()
    {
        var result = WindowPlacement.ClampToVisibleArea(new PointD(100, 900), DefaultWindowSize, PrimaryArea);

        Assert.Equal(new PointD(100, 600), result);
    }

    [Fact]
    public void ClampToVisibleArea_左にはみ出す場合は左端へ詰める()
    {
        var result = WindowPlacement.ClampToVisibleArea(new PointD(-100, 100), DefaultWindowSize, PrimaryArea);

        Assert.Equal(new PointD(0, 100), result);
    }

    [Fact]
    public void ClampToVisibleArea_上にはみ出す場合は上端へ詰める()
    {
        var result = WindowPlacement.ClampToVisibleArea(new PointD(100, -50), DefaultWindowSize, PrimaryArea);

        Assert.Equal(new PointD(100, 0), result);
    }

    [Theory]
    [InlineData(2000, 100)]
    [InlineData(100, 1300)]
    [InlineData(-900, 100)]
    [InlineData(100, -700)]
    public void ClampToVisibleArea_完全に領域外の場合は原点へ移動する(double x, double y)
    {
        var result = WindowPlacement.ClampToVisibleArea(new PointD(x, y), DefaultWindowSize, PrimaryArea);

        Assert.Equal(new PointD(0, 0), result);
    }

    [Theory]
    [InlineData(1920, 100)]
    [InlineData(100, 1200)]
    [InlineData(-800, 100)]
    public void ClampToVisibleArea_境界で接する場合は重なりとみなさない(double x, double y)
    {
        var result = WindowPlacement.ClampToVisibleArea(new PointD(x, y), DefaultWindowSize, PrimaryArea);

        Assert.Equal(new PointD(0, 0), result);
    }

    [Fact]
    public void ClampToVisibleArea_ウィンドウが領域より大きい場合は先頭へ寄せる()
    {
        var result = WindowPlacement.ClampToVisibleArea(new PointD(100, 100), new SizeD(2400, 1400), PrimaryArea);

        Assert.Equal(new PointD(0, 0), result);
    }

    [Fact]
    public void ClampToVisibleArea_負座標を含む領域内の位置はそのまま返す()
    {
        var result = WindowPlacement.ClampToVisibleArea(new PointD(-1500, 100), DefaultWindowSize, MultiMonitorArea);

        Assert.Equal(new PointD(-1500, 100), result);
    }

    [Fact]
    public void ClampToVisibleArea_負座標を含む領域の右端へ詰める()
    {
        var result = WindowPlacement.ClampToVisibleArea(new PointD(1500, 100), DefaultWindowSize, MultiMonitorArea);

        Assert.Equal(new PointD(1120, 100), result);
    }
}
