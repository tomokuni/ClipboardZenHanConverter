using EsUtil.ClipboardZenHanConverter.Core.Icons;
using Xunit;

namespace EsUtil.ClipboardZenHanConverter.Tests.Core.Icons;

/// <summary><see cref="FluentIconData"/> の SVG パスデータ提供を検証します。</summary>
public sealed class FluentIconDataTests
{
    /// <summary>24px グリッドの SVG は移動コマンドから始まります。</summary>
    private const char MoveCommand = 'M';

    [Fact]
    public void ConvertRange_SVGのパスデータを返す()
    {
        var pathData = FluentIconData.ConvertRange;

        Assert.False(string.IsNullOrWhiteSpace(pathData));
        Assert.Equal(MoveCommand, pathData[0]);
        Assert.DoesNotContain('<', pathData);
    }

    [Fact]
    public void Settings_SVGのパスデータを返す()
    {
        var pathData = FluentIconData.Settings;

        Assert.False(string.IsNullOrWhiteSpace(pathData));
        Assert.Equal(MoveCommand, pathData[0]);
        Assert.DoesNotContain('<', pathData);
    }

    [Fact]
    public void ConvertRange_同一インスタンスを返す()
    {
        Assert.Same(FluentIconData.ConvertRange, FluentIconData.ConvertRange);
    }

    [Fact]
    public void Settings_同一インスタンスを返す()
    {
        Assert.Same(FluentIconData.Settings, FluentIconData.Settings);
    }

    [Fact]
    public void アイコン毎に異なるパスデータを返す()
    {
        Assert.NotEqual(FluentIconData.ConvertRange, FluentIconData.Settings);
    }
}
