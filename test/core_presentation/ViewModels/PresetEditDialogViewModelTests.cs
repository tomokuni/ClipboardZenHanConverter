using ClipboardZenHanConverter.Core.Models;
using Xunit;

namespace ClipboardZenHanConverter.Tests.Presentation.ViewModels;

/// <summary><see cref="PresetEditDialogViewModel"/> の検証と保存/削除の可否を検証します。</summary>
/// <remarks>実ユーザーの設定ファイルを汚さないよう、自動保存先を一時ディレクトリへ差し替えます。</remarks>
public sealed class PresetEditDialogViewModelTests : IDisposable
{
    /// <summary>実ユーザーのプリセット保存先（テスト中は一時ディレクトリへ差し替えるため退避）。</summary>
    private static string? s_originalPresetDirectory;

    /// <summary>テスト専用の一時ディレクトリ。</summary>
    private readonly string _tempDirectory = TestHelper.CreateTempDirectory();

    /// <summary>検証に使用する変換設定。</summary>
    private readonly ConvertConfig _config;

    /// <summary>設定画面の ViewModel。</summary>
    private readonly SettingsViewModel _settings;

    /// <summary>テストインスタンスを初期化します。</summary>
    public PresetEditDialogViewModelTests()
    {
        s_originalPresetDirectory ??= ConvertConfig.PresetDirectory;
        ConvertConfig.PresetDirectory = Path.Combine(_tempDirectory, "Presets");

        _config = TestHelper.CreateDefaultConfig();
        _config.AutoSaveFileName = Path.Combine(_tempDirectory, "Settings.json");
        _settings = new SettingsViewModel(_config);
    }

    /// <summary>プリセット保存先を元へ戻し、一時ディレクトリを削除します。</summary>
    public void Dispose()
    {
        ConvertConfig.PresetDirectory = s_originalPresetDirectory ?? ConvertConfig.PresetDirectory;

        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, recursive: true);

        GC.SuppressFinalize(this);
    }

    /// <summary>検証対象のダイアログ ViewModel を作成します。</summary>
    /// <param name="name">初期のプリセット名。</param>
    /// <returns>作成されたダイアログ ViewModel。</returns>
    private PresetEditDialogViewModel Create(string name = "")
        => new(_settings) { PresetName = name };

    [Fact]
    public void 空文字は保存も削除もできない()
    {
        var vm = Create("");

        Assert.Null(vm.ErrorMessage);
        Assert.False(vm.CanSave);
        Assert.False(vm.CanDelete);
    }

    [Fact]
    public void 空白のみは保存も削除もできない()
    {
        var vm = Create("   ");

        Assert.Null(vm.ErrorMessage);
        Assert.False(vm.CanSave);
        Assert.False(vm.CanDelete);
    }

    [Fact]
    public void 新規の有効な名前は保存のみ可能()
    {
        var vm = Create("新しいプリセット");

        Assert.Null(vm.ErrorMessage);
        Assert.True(vm.CanSave);
        Assert.False(vm.CanDelete);
    }

    [Fact]
    public void 組込みプリセットは保存も削除もできない()
    {
        var vm = Create(ConvertConfig.BuiltInPresetAccountingPower);

        Assert.Equal("組込みプリセットです。", vm.ErrorMessage);
        Assert.False(vm.CanSave);
        Assert.False(vm.CanDelete);
    }

    [Theory]
    [InlineData("テスト:名前")]
    [InlineData("テスト/名前")]
    [InlineData("テスト\\名前")]
    public void 使用できない文字を含む名前は保存できない(string name)
    {
        var vm = Create(name);

        Assert.Equal("使用できない文字が含まれます。", vm.ErrorMessage);
        Assert.False(vm.CanSave);
    }

    [Theory]
    [InlineData("built-inテスト")]
    [InlineData("builtinテスト")]
    [InlineData("ＢＵＩＬＴ－ＩＮテスト")]
    [InlineData("BuiltInテスト")]
    public void builtinキーワードを含む名前は保存できない(string name)
    {
        var vm = Create(name);

        Assert.Equal("built-in または builtin は使用できません。", vm.ErrorMessage);
        Assert.False(vm.CanSave);
    }

    [Fact]
    public void 既存ユーザープリセットは保存も削除も可能()
    {
        _settings.SavePreset("既存プリセット");
        var vm = Create("既存プリセット");

        Assert.Equal("既に存在します。", vm.ErrorMessage);
        Assert.True(vm.CanSave);
        Assert.True(vm.CanDelete);

        _settings.DeletePreset("既存プリセット");
    }

    [Fact]
    public void 使用できない文字を含む名前は削除もできない()
    {
        // 使用不可文字を含む名前はプリセットとして保存できない（ファイル名にできない）ため、
        // 削除対象として存在し得ません。したがって削除も無効です。
        var vm = Create("テスト:名前");

        Assert.Equal("使用できない文字が含まれます。", vm.ErrorMessage);
        Assert.False(vm.CanSave);
        Assert.False(vm.CanDelete);
    }

    [Fact]
    public void 名前を変更すると検証結果が更新される()
    {
        var vm = Create("新しいプリセット");
        Assert.True(vm.CanSave);

        vm.PresetName = ConvertConfig.BuiltInPresetAccountingPower;

        Assert.False(vm.CanSave);
        Assert.Equal("組込みプリセットです。", vm.ErrorMessage);
    }

    [Fact]
    public void SaveCommand_プリセットを保存して閉じる()
    {
        var closed = false;
        var vm = Create("保存テスト");
        vm.SetCloseAction(() => closed = true);

        vm.SaveCommand.Execute(null);

        Assert.True(closed);
        Assert.Contains("保存テスト", _settings.PresetNames);

        _settings.DeletePreset("保存テスト");
    }

    [Fact]
    public void SaveCommand_保存できない場合は何もしない()
    {
        var closed = false;
        var vm = Create(ConvertConfig.BuiltInPresetAccountingPower);
        vm.SetCloseAction(() => closed = true);

        vm.SaveCommand.Execute(null);

        Assert.False(closed);
    }

    [Fact]
    public void DeleteCommand_プリセットを削除して閉じる()
    {
        _settings.SavePreset("削除テスト");
        var closed = false;
        var vm = Create("削除テスト");
        vm.SetCloseAction(() => closed = true);

        vm.DeleteCommand.Execute(null);

        Assert.True(closed);
        Assert.DoesNotContain("削除テスト", _settings.PresetNames);
    }

    [Fact]
    public void DeleteCommand_削除できない場合は何もしない()
    {
        var closed = false;
        var vm = Create("新規プリセット");
        vm.SetCloseAction(() => closed = true);

        vm.DeleteCommand.Execute(null);

        Assert.False(closed);
    }

    [Fact]
    public void CloseCommand_何もせず閉じる()
    {
        var closed = false;
        var vm = Create("プリセット");
        vm.SetCloseAction(() => closed = true);

        vm.CloseCommand.Execute(null);

        Assert.True(closed);
    }

    [Theory]
    [InlineData("abc", false)]
    [InlineData("a:b", true)]
    [InlineData("a/b", true)]
    public void ContainsInvalidFileNameChars_使用不可文字を判定する(string name, bool expected)
        => Assert.Equal(expected, PresetEditDialogViewModel.ContainsInvalidFileNameChars(name));

    [Theory]
    [InlineData("abc", false)]
    [InlineData("built-in", true)]
    [InlineData("Built-In", true)]
    [InlineData("builtin", true)]
    [InlineData("ＢＵＩＬＴＩＮ", true)]
    [InlineData("プリセット", false)]
    public void ContainsBuiltInKeyword_正規化して判定する(string name, bool expected)
        => Assert.Equal(expected, PresetEditDialogViewModel.ContainsBuiltInKeyword(name));
}
