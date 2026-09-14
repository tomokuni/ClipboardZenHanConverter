using ClipboardZenHanConverter.Core.Enums;
using ClipboardZenHanConverter.Core.Models;
using Xunit;

namespace ClipboardZenHanConverter.Tests.Presentation.ViewModels;

/// <summary><see cref="SettingsViewModel"/> の項目生成・設定同期・プリセット・入出力を検証します。</summary>
/// <remarks>ファイルを書き込むテストでは、実ユーザーの設定ファイルを汚さないよう<br/>
/// 自動保存先を一時ディレクトリへ差し替えます。</remarks>
public sealed class SettingsViewModelTests : IDisposable
{
    /// <summary>実ユーザーのプリセット保存先（テスト中は一時ディレクトリへ差し替えるため退避）。</summary>
    private static string? s_originalPresetDirectory;

    /// <summary>テスト専用の一時ディレクトリ。</summary>
    private readonly string _tempDirectory = TestHelper.CreateTempDirectory();

    /// <summary>検証に使用する変換設定。</summary>
    private readonly ConvertConfig _config;

    /// <summary>検証対象の ViewModel。</summary>
    private readonly SettingsViewModel _viewModel;

    /// <summary>テストインスタンスを初期化します。</summary>
    public SettingsViewModelTests()
    {
        s_originalPresetDirectory ??= ConvertConfig.PresetDirectory;
        ConvertConfig.PresetDirectory = Path.Combine(_tempDirectory, "Presets");

        _config = TestHelper.CreateDefaultConfig();
        _config.AutoSaveFileName = Path.Combine(_tempDirectory, "Settings.json");
        _viewModel = new SettingsViewModel(_config);
    }

    /// <summary>プリセット保存先を元へ戻し、一時ディレクトリを削除します。</summary>
    public void Dispose()
    {
        ConvertConfig.PresetDirectory = s_originalPresetDirectory ?? ConvertConfig.PresetDirectory;

        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, recursive: true);

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Constructor_各カテゴリの項目数が定義と一致する()
    {
        Assert.Equal(SegmentDefinitions.NumberDefs.Length, _viewModel.NumberItems.Count);
        Assert.Equal(SegmentDefinitions.AlphabetDefs.Length, _viewModel.AlphabetItems.Count);
        Assert.Equal(SegmentDefinitions.KanaDefs.Length, _viewModel.KanaItems.Count);
        Assert.Equal(SegmentDefinitions.SymbolDefs.Length, _viewModel.SymbolItems.Count);
        Assert.Equal(SegmentDefinitions.EtcZenHanAsciiDefs.Length, _viewModel.EtcZenHanAsciiItems.Count);
        Assert.Equal(SegmentDefinitions.EtcBslashYenDefs.Length, _viewModel.EtcBslashYenItems.Count);
        Assert.Equal(SegmentDefinitions.EtcSpecialDefs.Length, _viewModel.EtcSpecialItems.Count);
        Assert.Equal(SegmentDefinitions.EtcMultiSpaceDefs.Length, _viewModel.EtcMultiSpaceItems.Count);
    }

    [Fact]
    public void Constructor_全カテゴリの項目ラベルと選択肢が定義順と一致する()
    {
        // カテゴリごとに、生成された項目がセグメント定義と同順・同内容であることを確認します。
        AssertGroup(_viewModel.NumberItems, SegmentDefinitions.NumberDefs);
        AssertGroup(_viewModel.AlphabetItems, SegmentDefinitions.AlphabetDefs);
        AssertGroup(_viewModel.KanaItems, SegmentDefinitions.KanaDefs);
        AssertGroup(_viewModel.SymbolItems, SegmentDefinitions.SymbolDefs);
        AssertGroup(_viewModel.EtcZenHanAsciiItems, SegmentDefinitions.EtcZenHanAsciiDefs);
        AssertGroup(_viewModel.EtcBslashYenItems, SegmentDefinitions.EtcBslashYenDefs);
        AssertGroup(_viewModel.EtcSpecialItems, SegmentDefinitions.EtcSpecialDefs);
        AssertGroup(_viewModel.EtcMultiSpaceItems, SegmentDefinitions.EtcMultiSpaceDefs);
    }

    /// <summary>1 カテゴリの項目がセグメント定義と同順・同内容であることを検証します。</summary>
    /// <param name="items">生成された変換項目。</param>
    /// <param name="defs">対応するセグメント定義。</param>
    private static void AssertGroup(IList<ZenHanConvertItem> items, SegmentDefine[] defs)
    {
        Assert.Equal(defs.Length, items.Count);

        for (var i = 0; i < defs.Length; i++)
        {
            Assert.Equal(defs[i].Label, items[i].Label);

            // 定義が選択肢を指定しない場合は、既定の 3 択（なし/半角/全角）になります。
            var expected = defs[i].Segments is { Length: > 0 } segments
                ? segments.Select(s => s.Content)
                : ["なし", "半角", "全角"];

            Assert.Equal(expected, items[i].Options.Select(o => o.Content));
        }
    }

    [Fact]
    public void Constructor_置換ルールを読み込む()
    {
        var config = TestHelper.CreateDefaultConfig();
        config.AutoSaveFileName = Path.Combine(_tempDirectory, "Settings.json");
        config.ReplacePairs = [new ReplacePair("abc", "xyz", IsRegex: true)];

        var viewModel = new SettingsViewModel(config);

        Assert.Single(viewModel.ReplaceItems);
        Assert.Equal("abc", viewModel.ReplaceItems[0].Search);
        Assert.True(viewModel.ReplaceItems[0].IsRegex);
    }

    [Fact]
    public void Item_SelectedLabel変更でConfigが更新される()
    {
        _viewModel.NumberItems[0].SelectedLabel = "半角";

        Assert.Equal(ZenHanMode.ToHan, _config.ConvertModeNumber);
    }

    [Fact]
    public void Item_Config変更がSelectedLabelに反映される()
    {
        _config.ConvertModeNumber = ZenHanMode.ToZen;

        Assert.Equal("全角", _viewModel.NumberItems[0].SelectedLabel);
    }

    [Fact]
    public void SavePreset_空文字は何もしない()
    {
        var before = _viewModel.PresetNames.Count;

        _viewModel.SavePreset("   ");

        Assert.Equal(before, _viewModel.PresetNames.Count);
    }

    [Fact]
    public void DeletePreset_空文字は何もしない()
    {
        _viewModel.SavePreset("残すプリセット");
        var before = _viewModel.PresetNames.Count;

        _viewModel.DeletePreset("");
        _viewModel.DeletePreset(null);

        Assert.Equal(before, _viewModel.PresetNames.Count);
        Assert.Contains("残すプリセット", _viewModel.PresetNames);
    }

    [Theory]
    [InlineData(ConvertConfig.BuiltInPresetAccountingPower)]
    [InlineData(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen)]
    public void DeletePreset_組み込みプリセットは削除されない(string presetName)
    {
        // 組込みプリセットはユーザーデータではないため、削除操作でも消しません。
        _viewModel.DeletePreset(presetName);

        Assert.Contains(presetName, _viewModel.PresetNames);
    }

    [Fact]
    public void 置換行の編集は設定へ即時反映される()
    {
        _viewModel.AddReplaceRowCommand.Execute(null);
        var item = _viewModel.ReplaceItems[0];

        item.Search = "abc";
        item.Replace = "xyz";

        Assert.Single(_config.ReplacePairs);
        Assert.Equal("abc", _config.ReplacePairs[0].Search);
        Assert.Equal("xyz", _config.ReplacePairs[0].Replace);
    }

    [Fact]
    public void 置換行のバリデーションエラーはErrorMessageに反映される()
    {
        _viewModel.AddReplaceRowCommand.Execute(null);
        var item = _viewModel.ReplaceItems[0];

        item.Search = "";
        item.Replace = "";

        Assert.NotNull(item.ErrorMessage);
        Assert.Empty(_config.ReplacePairs);
    }

    [Fact]
    public void ValidateAllAndSyncToConfig_有効な行のみを反映する()
    {
        _viewModel.AddReplaceRowCommand.Execute(null);
        _viewModel.ReplaceItems[0].Search = "abc";
        _viewModel.ReplaceItems[0].Replace = "xyz";
        _viewModel.AddReplaceRowCommand.Execute(null);

        _viewModel.ValidateAllAndSyncToConfig();

        Assert.Single(_config.ReplacePairs);
        Assert.Equal("abc", _config.ReplacePairs[0].Search);
    }

    [Fact]
    public void AddReplaceRowCommand_空行を追加し無効行は反映しない()
    {
        _viewModel.AddReplaceRowCommand.Execute(null);

        Assert.Single(_viewModel.ReplaceItems);
        Assert.Empty(_config.ReplacePairs);
    }

    [Fact]
    public void DeleteReplaceRowCommand_行を削除して同期する()
    {
        _viewModel.AddReplaceRowCommand.Execute(null);
        _viewModel.ReplaceItems[0].Search = "abc";
        _viewModel.ReplaceItems[0].Replace = "xyz";
        var item = _viewModel.ReplaceItems[0];

        _viewModel.DeleteReplaceRowCommand.Execute(item);

        Assert.Empty(_viewModel.ReplaceItems);
        Assert.Empty(_config.ReplacePairs);
    }

    [Fact]
    public void DeleteReplaceRowCommand_nullを指定しても何もしない()
    {
        var ex = Record.Exception(() => _viewModel.DeleteReplaceRowCommand.Execute(null));

        Assert.Null(ex);
    }

    [Fact]
    public void ReloadReplaceItemsFromConfig_設定から再構築する()
    {
        _config.ReplacePairs = [new ReplacePair("abc", "xyz", IsRegex: true)];

        _viewModel.ReloadReplaceItemsFromConfig();

        Assert.Single(_viewModel.ReplaceItems);
        Assert.Equal("abc", _viewModel.ReplaceItems[0].Search);
        Assert.True(_viewModel.ReplaceItems[0].IsRegex);
    }

    [Fact]
    public void ReloadReplaceItemsFromConfig_再構築後も編集が設定へ反映される()
    {
        _config.ReplacePairs = [new ReplacePair("abc", "xyz")];
        _viewModel.ReloadReplaceItemsFromConfig();

        _viewModel.ReplaceItems[0].Search = "abc2";

        Assert.Equal("abc2", _config.ReplacePairs[0].Search);
    }

    [Fact]
    public void RefreshPresets_組み込みプリセットを含む()
    {
        _viewModel.RefreshPresets();

        Assert.Contains(ConvertConfig.BuiltInPresetAccountingPower, _viewModel.PresetNames);
        Assert.Contains(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen, _viewModel.PresetNames);
    }

    [Fact]
    public void SelectedPresetName_組み込みプリセットを選択すると設定が読み込まれる()
    {
        _viewModel.SelectedPresetName = ConvertConfig.BuiltInPresetAlphanumericHanKanaZen;

        Assert.Equal(ZenHanMode.ToHan, _config.ConvertModeNumber);
        Assert.Equal(ZenHanMode.ToHan, _config.ConvertModeAlphabet);
        Assert.Equal(ZenHanKanaMode.ToZenKata, _config.ConvertModeKanaHan);
        Assert.Equal(ConvertConfig.BuiltInPresetAlphanumericHanKanaZen, _viewModel.SelectedPresetName);
    }

    [Fact]
    public void 設定変更時に一致するプリセット名が自動設定される()
    {
        _viewModel.SelectedPresetName = ConvertConfig.BuiltInPresetAccountingPower;

        Assert.Equal(ConvertConfig.BuiltInPresetAccountingPower, _viewModel.SelectedPresetName);
    }

    [Fact]
    public void 一致しない設定ではプリセット名が未選択になる()
    {
        _viewModel.SelectedPresetName = ConvertConfig.BuiltInPresetAccountingPower;
        _config.ConvertModeNumber = ZenHanMode.None;
        _config.ConvertModeSymbolParenthesis = ZenHanMode.None;

        Assert.Null(_viewModel.SelectedPresetName);
    }

    [Fact]
    public void LoadPreset_組み込みプリセットを読み込む()
    {
        _viewModel.LoadPreset(ConvertConfig.BuiltInPresetAccountingPower);

        Assert.Equal(ZenHanMode.ToZen, _config.ConvertModeSymbolExclamation);
        Assert.Equal(ZenHanKanaMode.ToZenKata, _config.ConvertModeKanaHan);
        Assert.Equal(ZenHanEtcSpecial.ToHanSpace, _config.ConvertModeEtcNewline);
    }

    [Fact]
    public void LoadPreset_空文字は何もしない()
    {
        _config.ConvertModeNumber = ZenHanMode.ToZen;

        _viewModel.LoadPreset(string.Empty);
        _viewModel.LoadPreset(null);

        Assert.Equal(ZenHanMode.ToZen, _config.ConvertModeNumber);
    }

    [Fact]
    public void SavePreset_保存後にプリセット一覧へ現れる()
    {
        _config.ConvertModeNumber = ZenHanMode.ToZen;

        _viewModel.SavePreset("テスト用プリセット");

        Assert.Contains("テスト用プリセット", _viewModel.PresetNames);
        Assert.Equal("テスト用プリセット", _viewModel.SelectedPresetName);

        _viewModel.DeletePreset("テスト用プリセット");
    }

    [Fact]
    public void ExportSettings_ファイルを書き出す()
    {
        _config.ConvertModeNumber = ZenHanMode.ToHan;
        var file = Path.Combine(_tempDirectory, "export.json");

        _viewModel.ExportSettings(file);

        Assert.True(File.Exists(file));
        Assert.Contains("ConvertModeNumber", File.ReadAllText(file));
    }

    [Fact]
    public void ImportSettings_成功時はnullを返し設定を反映する()
    {
        var source = TestHelper.CreateDefaultConfig();
        source.ConvertModeNumber = ZenHanMode.ToZen;
        source.ReplacePairs = [new ReplacePair("abc", "xyz")];
        var file = Path.Combine(_tempDirectory, "import.json");
        source.ExportToFile(file);

        var error = _viewModel.ImportSettings(file);

        Assert.Null(error);
        Assert.Equal(ZenHanMode.ToZen, _config.ConvertModeNumber);
        Assert.Single(_viewModel.ReplaceItems);
    }

    [Fact]
    public void ImportSettings_失敗時はエラーメッセージを返す()
    {
        var file = Path.Combine(_tempDirectory, "broken.json");
        File.WriteAllText(file, "これはJSONではありません");

        var error = _viewModel.ImportSettings(file);

        Assert.NotNull(error);
    }
}
