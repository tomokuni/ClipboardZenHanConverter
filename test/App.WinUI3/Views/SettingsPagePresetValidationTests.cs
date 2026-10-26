using EsUtil.ZenHanConverter.App.WinUI3.Views;
using Xunit;

namespace EsUtil.ZenHanConverter.Tests.App.WinUI3.Views;

/// <summary>SettingsPagePresetValidationTests の検証項目を表します。</summary>
public class SettingsPagePresetValidationTests
{
    // ─── ContainsInvalidFileNameChars ───

    [Fact]
    internal void ContainsInvalidFileNameChars_通常の名前はfalse()
    {
        Assert.False(SettingsPage.ContainsInvalidFileNameChars("通常の名前"));
    }

    [Fact]
    internal void ContainsInvalidFileNameChars_日本語はfalse()
    {
        Assert.False(SettingsPage.ContainsInvalidFileNameChars("全力会計プリセット"));
    }

    [Fact]
    internal void ContainsInvalidFileNameChars_アルファベット数字はfalse()
    {
        Assert.False(SettingsPage.ContainsInvalidFileNameChars("MyPreset123"));
    }

    [Fact]
    internal void ContainsInvalidFileNameChars_空文字はfalse()
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
    internal void ContainsInvalidFileNameChars_使用不可文字を含むとtrue(char invalidChar)
    {
        Assert.True(SettingsPage.ContainsInvalidFileNameChars($"preset{invalidChar}name"));
    }

    // ─── ContainsBuiltInKeyword ───

    [Fact]
    internal void ContainsBuiltInKeyword_小文字built_inはtrue()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("built-in"));
    }

    [Fact]
    internal void ContainsBuiltInKeyword_小文字builtinはtrue()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("builtin"));
    }

    [Fact]
    internal void ContainsBuiltInKeyword_先頭大文字BuiltInはtrue()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("BuiltIn"));
    }

    [Fact]
    internal void ContainsBuiltInKeyword_大文字BUILT_INはtrue()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("BUILT-IN"));
    }

    [Fact]
    internal void ContainsBuiltInKeyword_プリセット名の一部にbuilt_inを含む()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("my-built-in-preset"));
    }

    [Fact]
    internal void ContainsBuiltInKeyword_プリセット名の一部にbuiltinを含む()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("mybuiltinpreset"));
    }

    [Fact]
    internal void ContainsBuiltInKeyword_全角built_inはtrue()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("ｂｕｉｌｔ－ｉｎ"));
    }

    [Fact]
    internal void ContainsBuiltInKeyword_全角大文字BUILTINはtrue()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("ＢＵＩＬＴＩＮ"));
    }

    [Fact]
    internal void ContainsBuiltInKeyword_全角小文字builtinはtrue()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("ｂｕｉｌｔｉｎ"));
    }

    [Fact]
    internal void ContainsBuiltInKeyword_末尾にbuilt_inがある()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("prebuilt-in"));
    }

    [Fact]
    internal void ContainsBuiltInKeyword_末尾に小文字builtinがある()
    {
        Assert.True(SettingsPage.ContainsBuiltInKeyword("prebuiltin"));
    }

    [Fact]
    internal void ContainsBuiltInKeyword_通常の名前はfalse()
    {
        Assert.False(SettingsPage.ContainsBuiltInKeyword("通常の名前"));
    }

    [Fact]
    internal void ContainsBuiltInKeyword_空文字はfalse()
    {
        Assert.False(SettingsPage.ContainsBuiltInKeyword(""));
    }

    [Fact]
    internal void ContainsBuiltInKeyword_builtのみはfalse()
    {
        Assert.False(SettingsPage.ContainsBuiltInKeyword("built"));
    }

    [Fact]
    internal void ContainsBuiltInKeyword_builtのみを含むプリセット名はfalse()
    {
        Assert.False(SettingsPage.ContainsBuiltInKeyword("mybuiltpreset"));
    }

    [Fact]
    internal void ContainsBuiltInKeyword_部分一致でない通常名はfalse()
    {
        Assert.False(SettingsPage.ContainsBuiltInKeyword("通常のbuiltではありません"));
    }
}
