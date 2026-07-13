using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClipboardZenHanConverter.Core.Enums;
using ClipboardZenHanConverter.Core.Models;

namespace ClipboardZenHanConverter.ViewModels;

/// <summary>設定画面のデータを管理するViewModelクラスです。</summary>
public partial class SettingsViewModel : ObservableObject
{
    public ConvertConfig ConvertConfig { get; }
    public IList<ZenHanConvertItem> NumberItems { get; }
    public IList<ZenHanConvertItem> AlphabetItems { get; }
    public IList<ZenHanConvertItem> KanaItems { get; }
    public IList<ZenHanConvertItem> SymbolItems { get; }
    public IList<ZenHanConvertItem> EtcZenHanAsciiItems { get; }
    public IList<ZenHanConvertItem> EtcBslashYenItems { get; }
    public IList<ZenHanConvertItem> EtcSpecialItems { get; }
    public IList<ZenHanConvertItem> EtcMultiSpaceItems { get; }

    /// <summary>利用可能なプリセット名の一覧。</summary>
    public ObservableCollection<string> PresetNames { get; } = [];

    /// <summary>現在編集中のプリセット名（保存時の初期値）。</summary>
    [ObservableProperty]
    public partial string NewPresetName { get; set; } = string.Empty;

    /// <summary>文字列置換テーブルの編集項目一覧。</summary>
    public ObservableCollection<ReplacePairItem> ReplaceItems { get; } = [];

    public SettingsViewModel(ConvertConfig convertConfig, SettingsModel model)
    {
        ConvertConfig = convertConfig;
        NumberItems = [.. model.NumberDefs.Select(d => new ZenHanConvertItem(convertConfig, d))];
        AlphabetItems = [.. model.AlphabetDefs.Select(d => new ZenHanConvertItem(convertConfig, d))];
        KanaItems = [.. model.KanaDefs.Select(d => new ZenHanConvertItem(convertConfig, d))];
        SymbolItems = [.. model.SymbolDefs.Select(d => new ZenHanConvertItem(convertConfig, d))];
        EtcZenHanAsciiItems = [.. model.EtcZenHanAsciiDefs.Select(d => new ZenHanConvertItem(convertConfig, d))];
        EtcBslashYenItems = [.. model.EtcBslashYenDefs.Select(d => new ZenHanConvertItem(convertConfig, d))];
        EtcSpecialItems = [.. model.EtcSpecialDefs.Select(d => new ZenHanConvertItem(convertConfig, d))];
        EtcMultiSpaceItems = [.. model.EtcMultiSpaceDefs.Select(d => new ZenHanConvertItem(convertConfig, d))];

        // 置換テーブル初期化（PropertyChanged 無し）
        foreach (var pair in convertConfig.ReplacePairs)
        {
            ReplaceItems.Add(new ReplacePairItem(pair));
        }

        RefreshPresets();
    }

    /// <summary>全ての置換行をバリデーションし、有効な行のみを ConvertConfig に同期します。</summary>
    public void ValidateAllAndSyncToConfig()
    {
        ConvertConfig.ReplacePairs = [.. ReplaceItems
            .Where(i => i.Validate() is null)
            .Select(i => i.ToPair())];
    }

    /// <summary>指定された1行をバリデーションし、全行を同期します。</summary>
    public void ValidateItemAndSync(ReplacePairItem item)
    {
        item.Validate();
        ValidateAllAndSyncToConfig();
    }

    /// <summary>プリセット読み込み後などに ReplaceItems を ConvertConfig から再構築します。</summary>
    public void ReloadReplaceItemsFromConfig()
    {
        ReplaceItems.Clear();
        foreach (var pair in ConvertConfig.ReplacePairs)
        {
            ReplaceItems.Add(new ReplacePairItem(pair));
        }
    }

    /// <summary>置換行を追加します。</summary>
    [RelayCommand]
    private void AddReplaceRow()
    {
        ReplaceItems.Add(new ReplacePairItem());
        ValidateAllAndSyncToConfig();
    }

    /// <summary>指定された置換行を削除します。</summary>
    [RelayCommand]
    private void DeleteReplaceRow(ReplacePairItem? item)
    {
        if (item is null) return;
        ReplaceItems.Remove(item);
        ValidateAllAndSyncToConfig();
    }

    /// <summary>設定をJSONファイルにエクスポートします。</summary>
    /// <param name="filePath">エクスポート先のファイルパス</param>
    public void ExportSettings(string filePath) => ConvertConfig.ExportToFile(filePath);

    /// <summary>設定をJSONファイルからインポートします。</summary>
    /// <param name="filePath">インポート元のファイルパス</param>
    /// <returns>インポートに成功したかどうかを示す文字列（成功時はnull、失敗時はエラーメッセージ）</returns>
    public string? ImportSettings(string filePath)
    {
        var result = ConvertConfig.ImportFromFile(filePath);
        if (result)
        {
            ReloadReplaceItemsFromConfig();
            return null;
        }
        return "設定のインポートに失敗しました。ファイルが正しいJSON形式であることを確認してください。";
    }

    /// <summary>プリセット一覧を更新します。</summary>
    [RelayCommand]
    private void RefreshPresets()
    {
        PresetNames.Clear();
        foreach (var name in ConvertConfig.GetPresetNames())
        {
            PresetNames.Add(name);
        }
    }

    /// <summary>現在の設定をプリセットとして保存します。</summary>
    [RelayCommand]
    private void SavePreset()
    {
        var name = NewPresetName?.Trim();
        if (string.IsNullOrEmpty(name)) return;

        ConvertConfig.SavePreset(name);
        NewPresetName = string.Empty;
        RefreshPresets();
    }

    /// <summary>プリセットを読み込みます。</summary>
    [RelayCommand]
    private void LoadPreset(string? name)
    {
        if (string.IsNullOrEmpty(name)) return;
        ConvertConfig.LoadPreset(name);
        ReloadReplaceItemsFromConfig();
    }

    /// <summary>プリセットを削除します。</summary>
    [RelayCommand]
    private void DeletePreset(string? name)
    {
        if (string.IsNullOrEmpty(name)) return;
        ConvertConfig.DeletePreset(name);
        RefreshPresets();
    }
}

