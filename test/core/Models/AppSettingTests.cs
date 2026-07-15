using ClipboardZenHanConverter.Core.Models;
using Xunit;

namespace ClipboardZenHanConverter.Tests.Core.Models;

public class AppSettingTests
{
    [Fact]
    public void Constructor_デフォルト値()
    {
        var setting = new AppSetting();
        Assert.Equal(1000.0, setting.WindowWidth);
        Assert.Equal(800.0, setting.WindowHeight);
        Assert.Contains("AppSetting.json", setting.AutoSaveFileName);
    }

    [Fact]
    public void ウィンドウサイズの変更()
    {
        var setting = new AppSetting();
        setting.WindowWidth = 1920;
        setting.WindowHeight = 1080;

        Assert.Equal(1920.0, setting.WindowWidth);
        Assert.Equal(1080.0, setting.WindowHeight);
    }
}
