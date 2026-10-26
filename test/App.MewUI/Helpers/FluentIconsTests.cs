using EsUtil.ZenHanConverter.App.MewUI.Helpers;
using Xunit;

namespace EsUtil.ZenHanConverter.Tests.App.MewUI.Helpers;

/// <summary><see cref="FluentIcons"/> のアイコン形状生成を検証します。</summary>
/// <remarks>Core の SVG パスデータから MewUI の形状が生成できることを確認します。</remarks>
public sealed class FluentIconsTests
{
    [Fact]
    internal void ConvertRange_Coreのパスデータから形状を生成できる()
    {
        var geometry = FluentIcons.ConvertRange;

        Assert.NotNull(geometry);
    }

    [Fact]
    internal void Settings_Coreのパスデータから形状を生成できる()
    {
        var geometry = FluentIcons.Settings;

        Assert.NotNull(geometry);
    }

    [Fact]
    internal void ConvertRange_同一インスタンスを返す()
    {
        Assert.Same(FluentIcons.ConvertRange, FluentIcons.ConvertRange);
    }

    [Fact]
    internal void Settings_同一インスタンスを返す()
    {
        Assert.Same(FluentIcons.Settings, FluentIcons.Settings);
    }
}
