using ClipboardZenHanConverter.Core.Models;
using Xunit;

namespace ClipboardZenHanConverter.Tests.Core.Models;

/// <summary><see cref="AppSetting"/> の既定値と JSON からの復元を検証します。</summary>
public sealed class AppSettingTests : IDisposable
{
    private readonly string _tempDirectory = TestHelper.CreateTempDirectory();

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, recursive: true);

        GC.SuppressFinalize(this);
    }

    /// <summary>自動保存先を一時ディレクトリへ逃がした設定を作成します。</summary>
    /// <returns>自動保存先を差し替えた AppSetting。</returns>
    private AppSetting CreateSetting()
    {
        var setting = new AppSetting();
        setting.AutoSaveFileName = Path.Combine(_tempDirectory, "AppSetting.json");
        return setting;
    }

    [Fact]
    public void Constructor_既定値()
    {
        var setting = new AppSetting();

        Assert.Equal(1000.0, setting.WindowWidth);
        Assert.Equal(800.0, setting.WindowHeight);
        Assert.Null(setting.WindowX);
        Assert.Null(setting.WindowY);
        Assert.True(setting.IsClipboardConvertEnabled);
        Assert.Contains("AppSetting.json", setting.AutoSaveFileName);
    }

    [Fact]
    public void ウィンドウの位置とサイズを保持する()
    {
        var setting = CreateSetting();

        setting.WindowWidth = 1920;
        setting.WindowHeight = 1080;
        setting.WindowX = 120.5;
        setting.WindowY = 60.25;

        Assert.Equal(1920.0, setting.WindowWidth);
        Assert.Equal(1080.0, setting.WindowHeight);
        Assert.Equal(120.5, setting.WindowX);
        Assert.Equal(60.25, setting.WindowY);
    }

    [Fact]
    public void LoadFromJsonFile_保存した値を復元する()
    {
        var source = CreateSetting();
        source.WindowWidth = 1280;
        source.WindowHeight = 720;
        source.WindowX = 300;
        source.WindowY = 150;
        source.IsClipboardConvertEnabled = false;
        var file = Path.Combine(_tempDirectory, "round-trip.json");
        source.SaveToJsonFile(file);

        var restored = CreateSetting();
        restored.LoadFromJsonFile(file);

        Assert.Equal(1280.0, restored.WindowWidth);
        Assert.Equal(720.0, restored.WindowHeight);
        Assert.Equal(300.0, restored.WindowX);
        Assert.Equal(150.0, restored.WindowY);
        Assert.False(restored.IsClipboardConvertEnabled);
    }

    [Fact]
    public void LoadFromJsonFile_位置が未保存のJSONでは位置はnullのまま()
    {
        var file = Path.Combine(_tempDirectory, "no-position.json");
        File.WriteAllText(file, """{ "WindowWidth": 800, "WindowHeight": 600 }""");

        var setting = CreateSetting();
        setting.LoadFromJsonFile(file);

        Assert.Equal(800.0, setting.WindowWidth);
        Assert.Null(setting.WindowX);
        Assert.Null(setting.WindowY);
    }

    [Fact]
    public void LoadFromJsonFile_存在しないファイルは設定を維持する()
    {
        var setting = CreateSetting();
        setting.WindowWidth = 1111;

        setting.LoadFromJsonFile(Path.Combine(_tempDirectory, "not-found.json"));

        Assert.Equal(1111.0, setting.WindowWidth);
    }

    [Fact]
    public void LoadFromJsonFile_不正なJSONは設定を維持する()
    {
        var setting = CreateSetting();
        setting.WindowWidth = 1111;
        var file = Path.Combine(_tempDirectory, "broken.json");
        File.WriteAllText(file, "{ not json");

        setting.LoadFromJsonFile(file);

        Assert.Equal(1111.0, setting.WindowWidth);
    }
}
