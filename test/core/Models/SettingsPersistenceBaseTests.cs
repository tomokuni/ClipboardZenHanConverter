using EsUtil.ClipboardZenHanConverter.Core.Models;
using System;
using System.IO;
using Xunit;

namespace EsUtil.ClipboardZenHanConverter.Tests.Core.Models;

/// <summary><see cref="SettingsPersistenceBase{TSettings}"/> の JSON 入出力とリソース解放を検証します。</summary>
/// <remarks>抽象クラスのため、具象型である <see cref="AppSetting"/> を通じて検証します。</remarks>
public sealed class SettingsPersistenceBaseTests : IDisposable
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
    public void IsAutoSave_初期状態はfalse()
    {
        Assert.False(CreateSetting().IsAutoSave);
    }

    [Fact]
    public void SaveToJsonFile_インデント付きJSONを書き出す()
    {
        var setting = CreateSetting();
        setting.WindowWidth = 1234;
        var file = Path.Combine(_tempDirectory, "saved.json");

        setting.SaveToJsonFile(file);

        var json = File.ReadAllText(file);
        Assert.Contains("WindowWidth", json);
        Assert.Contains(Environment.NewLine, json);
    }

    [Fact]
    public void SaveToJsonFile_存在しないディレクトリでも例外を送出しない()
    {
        var setting = CreateSetting();
        var file = Path.Combine(_tempDirectory, "missing-dir", "saved.json");

        var ex = Record.Exception(() => setting.SaveToJsonFile(file));

        Assert.Null(ex);
    }

    [Fact]
    public void CancelPendingSave_保留が無くても例外を送出しない()
    {
        var setting = CreateSetting();

        var ex = Record.Exception(() => setting.CancelPendingSave());

        Assert.Null(ex);
    }

    [Fact]
    public void CancelPendingSave_複数回呼び出しても例外を送出しない()
    {
        var setting = CreateSetting();
        setting.CancelPendingSave();

        var ex = Record.Exception(() => setting.CancelPendingSave());

        Assert.Null(ex);
    }

    [Fact]
    public void Dispose_複数回呼び出しても例外を送出しない()
    {
        var setting = CreateSetting();
        setting.Dispose();

        var ex = Record.Exception(() => setting.Dispose());

        Assert.Null(ex);
    }

    [Fact]
    public void Dispose後もプロパティ変更で例外を送出しない()
    {
        var setting = CreateSetting();
        setting.Dispose();

        var ex = Record.Exception(() => setting.WindowWidth = 999);

        Assert.Null(ex);
    }
}
