using EsUtil.ZenHanConverter.App.WinUI3.Converters;
using Microsoft.UI.Xaml;
using System;
using Xunit;

namespace EsUtil.ZenHanConverter.Tests.App.WinUI3.Converters;

/// <summary>StringNotEmptyToVisibilityConverterTests の検証項目を表します。</summary>
public class StringNotEmptyToVisibilityConverterTests
{
    private readonly StringNotEmptyToVisibilityConverter _converter = new();

    [Fact]
    internal void Convert_通常文字列_Visible()
    {
        var result = _converter.Convert("abc", null!, null!, null!);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    internal void Convert_空文字_Collapsed()
    {
        var result = _converter.Convert("", null!, null!, null!);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    internal void Convert_null_Collapsed()
    {
        var result = _converter.Convert(null, null!, null!, null!);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    internal void Convert_空白文字_Visible()
    {
        var result = _converter.Convert("   ", null!, null!, null!);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    internal void ConvertBack_NotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() =>
            _converter.ConvertBack(null!, null!, null!, null!));
    }
}
