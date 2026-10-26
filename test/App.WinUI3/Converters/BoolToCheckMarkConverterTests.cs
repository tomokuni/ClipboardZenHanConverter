using EsUtil.ZenHanConverter.App.WinUI3.Converters;
using System;
using Xunit;

namespace EsUtil.ZenHanConverter.Tests.App.WinUI3.Converters;

/// <summary>BoolToCheckMarkConverterTests の検証項目を表します。</summary>
public class BoolToCheckMarkConverterTests
{
    private readonly BoolToCheckMarkConverter _converter = new();

    [Fact]
    internal void Convert_true_チェックマーク()
    {
        var result = _converter.Convert(true, null!, null!, null!);
        Assert.Equal("✅", result);
    }

    [Fact]
    internal void Convert_false_四角()
    {
        var result = _converter.Convert(false, null!, null!, null!);
        Assert.Equal("□", result);
    }

    [Fact]
    internal void Convert_null_四角()
    {
        var result = _converter.Convert(null, null!, null!, null!);
        Assert.Equal("□", result);
    }

    [Fact]
    internal void ConvertBack_NotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() =>
            _converter.ConvertBack(null!, null!, null!, null!));
    }
}
