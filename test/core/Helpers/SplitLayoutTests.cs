using ClipboardZenHanConverter.Core.Helpers;
using Xunit;

namespace ClipboardZenHanConverter.Tests.Core.Helpers;

/// <summary><see cref="SplitLayout"/> の分割比率の計算と補正を検証します。</summary>
/// <remarks>4 つの UI で共通に使用するため、上下限や境界条件をここで固定します。</remarks>
public sealed class SplitLayoutTests
{
    /// <summary>検証に使用する合計高さ（論理ピクセル）。</summary>
    private const double TrackHeight = 400;

    [Fact]
    public void DefaultRatio_は上下を等分する()
    {
        Assert.Equal(0.5, SplitLayout.DefaultRatio);
    }

    [Theory]
    [InlineData(0.0, SplitLayout.MinRatio)]
    [InlineData(-1.0, SplitLayout.MinRatio)]
    [InlineData(0.1, 0.1)]
    [InlineData(0.5, 0.5)]
    [InlineData(0.9, 0.9)]
    [InlineData(1.0, SplitLayout.MaxRatio)]
    [InlineData(2.0, SplitLayout.MaxRatio)]
    public void ClampRatio_は上下限の範囲へ補正する(double input, double expected)
    {
        Assert.Equal(expected, SplitLayout.ClampRatio(input));
    }

    [Fact]
    public void ClampRatio_非数は既定値へ補正する()
    {
        Assert.Equal(SplitLayout.DefaultRatio, SplitLayout.ClampRatio(double.NaN));
    }

    [Theory]
    [InlineData(SplitLayout.MinRatio)]
    [InlineData(0.5)]
    [InlineData(SplitLayout.MaxRatio)]
    public void TopWeightとBottomWeight_の合計は1になる(double ratio)
    {
        var total = SplitLayout.TopWeight(ratio) + SplitLayout.BottomWeight(ratio);

        Assert.Equal(1.0, total, 10);
    }

    [Fact]
    public void RatioFromDrag_移動量を比率へ換算する()
    {
        // 合計高さ 400 に対して下へ 100 移動すると、比率は 0.25 増える
        Assert.Equal(0.75, SplitLayout.RatioFromDrag(0.5, 100, TrackHeight), 10);
    }

    [Fact]
    public void RatioFromDrag_上方向の移動は比率を減らす()
    {
        Assert.Equal(0.25, SplitLayout.RatioFromDrag(0.5, -100, TrackHeight), 10);
    }

    [Fact]
    public void RatioFromDrag_移動量が0の場合は開始比率を返す()
    {
        Assert.Equal(0.5, SplitLayout.RatioFromDrag(0.5, 0, TrackHeight), 10);
    }

    [Fact]
    public void RatioFromDrag_下限を下回る移動は下限で止まる()
    {
        Assert.Equal(SplitLayout.MinRatio, SplitLayout.RatioFromDrag(0.5, -TrackHeight, TrackHeight), 10);
    }

    [Fact]
    public void RatioFromDrag_上限を上回る移動は上限で止まる()
    {
        Assert.Equal(SplitLayout.MaxRatio, SplitLayout.RatioFromDrag(0.5, TrackHeight, TrackHeight), 10);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void RatioFromDrag_高さが0以下の場合は開始比率を返す(double trackHeight)
    {
        Assert.Equal(0.5, SplitLayout.RatioFromDrag(0.5, 50, trackHeight), 10);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void RatioFromDrag_移動量が非数の場合は開始比率を返す(double deltaY)
    {
        Assert.Equal(0.5, SplitLayout.RatioFromDrag(0.5, deltaY, TrackHeight), 10);
    }

    [Fact]
    public void RatioFromDrag_開始比率が範囲外でも補正して計算する()
    {
        // 開始比率 1.0 は上限 0.9 に補正され、そこから 400 移動（比率にして +1.0）して上限で止まる
        Assert.Equal(SplitLayout.MaxRatio, SplitLayout.RatioFromDrag(1.0, TrackHeight, TrackHeight), 10);
    }
}
