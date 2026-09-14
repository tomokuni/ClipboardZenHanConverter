using ClipboardZenHanConverter.App.WinUI.Converters;
using Xunit;

namespace ClipboardZenHanConverter.Tests.App.WinUI.Converters;

public class BoolToCheckMarkConverterTests
{
    private readonly BoolToCheckMarkConverter _converter = new();

    [Fact]
    public void Convert_true_チェックマーク()
    {
        var result = _converter.Convert(true, null!, null!, null!);
        Assert.Equal("✅", result);
    }

    [Fact]
    public void Convert_false_四角()
    {
        var result = _converter.Convert(false, null!, null!, null!);
        Assert.Equal("□", result);
    }

    [Fact]
    public void Convert_null_四角()
    {
        var result = _converter.Convert(null, null!, null!, null!);
        Assert.Equal("□", result);
    }

    [Fact]
    public void ConvertBack_NotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() =>
            _converter.ConvertBack(null!, null!, null!, null!));
    }
}
