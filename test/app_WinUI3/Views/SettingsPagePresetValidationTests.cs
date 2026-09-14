using EsUtil.ClipboardZenHanConverter.App.WinUI.Views;
using Xunit;

namespace EsUtil.ClipboardZenHanConverter.Tests.App.WinUI.Views;

public class SettingsPagePresetValidationTests
{
    // ─── ContainsInvalidFileNameChars ───

    [Fact]
    public void ContainsInvalidFileNameChars_通常の名前はfalse()
    {
        Assert.False(SettingsPage.ContainsInvalidFileNameChars("通常の名前"));
    }

    [Fact]
    public void ContainsInvalidFileNameChars_日本語はfalse()
    {
        Assert.False(SettingsPage.ContainsInvalidFileNameChars("全力会計プリセット"));
    }

    [Fact]
    public void ContainsInvalidFileNameChars_アルファベット数字はfalse()
    {
        Assert.False(SettingsPage.ContainsInvalidFileNameChars("MyPreset123"));
    }

    [Fact]
    public void ContainsInvalidFileNameChars_空文字はfalse()
    {
        Assert.False(SettingsPage.ContainsInvalidFileNameChars(""));
    }

    [Theory]
    [InlineData('\\')]
    [InlineData('/')]
    [InlineData(':')]
    [InlineData('*')]
    [InlineData('?')]
    [InlineData('"')]
    [InlineData('<')]
    [InlineData('>')]
    [InlineData('|')]
    public void ContainsInvalidFileNameChars_使用不可文字を含むとtrue(char invalidChar)
    {
        Assert.True(SettingsPage.ContainsInvalidFileNameChars($"preset{invalidChar}name"));
    }

    // ─── ContainsBuiltInKeyword ───

    [Fact]
    public void ContainsBuiltInKeyword_built_inはtrue()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("built-in"));
    }

    [Fact]
    public void ContainsBuiltInKeyword_builtinはtrue()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("builtin"));
    }

    [Fact]
    public void ContainsBuiltInKeyword_BuiltInはtrue()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("BuiltIn"));
    }

    [Fact]
    public void ContainsBuiltInKeyword_BUILT_INはtrue()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("BUILT-IN"));
    }

    [Fact]
    public void ContainsBuiltInKeyword_プリセット名の一部にbuilt_inを含む()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("my-built-in-preset"));
    }

    [Fact]
    public void ContainsBuiltInKeyword_プリセット名の一部にbuiltinを含む()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("mybuiltinpreset"));
    }

    [Fact]
    public void ContainsBuiltInKeyword_全角built_inはtrue()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("ｂｕｉｌｔ－ｉｎ"));
    }

    [Fact]
    public void ContainsBuiltInKeyword_全角大文字BUILTINはtrue()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("ＢＵＩＬＴＩＮ"));
    }

    [Fact]
    public void ContainsBuiltInKeyword_全角builtinはtrue()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("ｂｕｉｌｔｉｎ"));
    }

    [Fact]
    public void ContainsBuiltInKeyword_末尾にbuilt_inがある()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("prebuilt-in"));
    }

    [Fact]
    public void ContainsBuiltInKeyword_末尾にbuiltinがある()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("prebuiltin"));
    }

    [Fact]
    public void ContainsBuiltInKeyword_通常の名前はfalse()
    {
        Assert.False(SettingsPage.ContainsBuiltInKeyword("通常の名前"));
    }

    [Fact]
    public void ContainsBuiltInKeyword_空文字はfalse()
    {
        Assert.False(SettingsPage.ContainsBuiltInKeyword(""));
    }

    [Fact]
    public void ContainsBuiltInKeyword_builtのみはfalse()
    {
        Assert.False(SettingsPage.ContainsBuiltInKeyword("built"));
    }

    [Fact]
    public void ContainsBuiltInKeyword_builtのみを含むプリセット名はfalse()
    {
        Assert.False(SettingsPage.ContainsBuiltInKeyword("mybuiltpreset"));
    }

    [Fact]
    public void ContainsBuiltInKeyword_部分一致でない通常名はfalse()
    {
        Assert.False(SettingsPage.ContainsBuiltInKeyword("通常のbuiltではありません"));
    }
}
