using ClipboardZenHanConverter.App.Converters;
using Microsoft.UI.Xaml;
using Xunit;

namespace ClipboardZenHanConverter.Tests.App.Converters;

public class StringNotEmptyToVisibilityConverterTests
{
    private readonly StringNotEmptyToVisibilityConverter _converter = new();

    [Fact]
    public void Convert_通常文字列_Visible()
    {
        var result = _converter.Convert("abc", null!, null!, null!);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_空文字_Collapsed()
    {
        var result = _converter.Convert("", null!, null!, null!);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_null_Collapsed()
    {
        var result = _converter.Convert(null, null!, null!, null!);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_空白文字_Visible()
    {
        var result = _converter.Convert("   ", null!, null!, null!);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void ConvertBack_NotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() =>
            _converter.ConvertBack(null!, null!, null!, null!));
    }
}
