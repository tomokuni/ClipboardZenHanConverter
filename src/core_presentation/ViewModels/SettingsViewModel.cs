using EsUtil.ClipboardZenHanConverter.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace EsUtil.ClipboardZenHanConverter.Presentation.ViewModels;

/// <summary>設定画面のデータを管理します。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - 8つの変換カテゴリ（数字/英字/かな/記号/約物/BS/YEN/特殊）のセグメント選択<br/>
/// - ユーザー定義の置換ルールの追加/編集/削除<br/>
/// - プリセットの保存/読み込み/削除/一覧表示<br/>
/// - 設定の JSON ファイルへのエクスポート/インポート<br/><br/>
/// 特徴: <br/>
/// - Core の SegmentDefinitions が提供するセグメント定義配列から動的に <see cref="ZenHanConvertItem"/> を生成する<br/>
/// - 置換ルールの編集は <see cref="ReplacePairItem"/> の変更通知を購読して即時検証し、設定へ反映する
///   （UI 側での検証呼び出しが不要）<br/>
/// - プリセット選択は <see cref="SelectedPresetName"/> を単一所有元とし、タイトルバーと設定画面が共有する<br/><br/>
/// 注意点: <br/>
/// - <see cref="ValidateAllAndSyncToConfig"/> と <see cref="ValidateItemAndSync"/> は公開していますが、
///   通常は編集内容の変更通知から自動で呼ばれるため、UI 側から明示的に呼ぶ必要はありません
/// </remarks>
public partial class SettingsViewModel : ObservableObject
{
    /// <summary>変換設定への参照を取得します。</summary>
    public ConvertConfig ConvertConfig { get; }

    /// <summary>数字変換のセグメントアイテム一覧。</summary>
    public IList<ZenHanConvertItem> NumberItems { get; }

    /// <summary>英字変換のセグメントアイテム一覧。</summary>
    public IList<ZenHanConvertItem> AlphabetItems { get; }

    /// <summary>かな変換のセグメントアイテム一覧。</summary>
    public IList<ZenHanConvertItem> KanaItems { get; }

    /// <summary>記号変換のセグメントアイテム一覧。</summary>
    public IList<ZenHanConvertItem> SymbolItems { get; }

    /// <summary>約物(長音/読点/句点)変換のセグメントアイテム一覧。</summary>
    public IList<ZenHanConvertItem> EtcZenHanAsciiItems { get; }

    /// <summary>バックスラッシュ/円記号変換のセグメントアイテム一覧。</summary>
    public IList<ZenHanConvertItem> EtcBslashYenItems { get; }

    /// <summary>特殊文字(タブ/改行)変換のセグメントアイテム一覧。</summary>
    public IList<ZenHanConvertItem> EtcSpecialItems { get; }

    /// <summary>連続スペース変換のセグメントアイテム一覧。</summary>
    public IList<ZenHanConvertItem> EtcMultiSpaceItems { get; }

    /// <summary>プリセット名の一覧。</summary>
    public ObservableCollection<string> PresetNames { get; } = [];

    /// <summary>ユーザー定義の置換ルール一覧。</summary>
    public ObservableCollection<ReplacePairItem> ReplaceItems { get; } = [];

    /// <summary>現在選択されているプリセット名を取得または設定します。</summary>
    /// <remarks>UI のドロップダウンで選択されたプリセット名を保持します。<br/>
    /// 値が変更されるとドロップダウンからの選択としてプリセットを読み込みます。<br/>
    /// 設定変更時は FindMatchingPreset で一致するプリセット名を自動設定します。</remarks>
    [ObservableProperty]
    public partial string? SelectedPresetName { get; set; }

    /// <summary>プリセット選択の更新中フラグ。再帰的なプリセット読み込みを防止します。</summary>
    private bool _isUpdatingSelection;

    /// <summary>SettingsViewModel の新しいインスタンスを初期化します。</summary>
    /// <param name="convertConfig">変換設定。この設定のセグメント定義に基づいて各変換項目を生成します。</param>
    public SettingsViewModel(ConvertConfig convertConfig)
    {
        ConvertConfig = convertConfig;
        NumberItems = CreateItems(convertConfig, SegmentDefinitions.NumberDefs);
        AlphabetItems = CreateItems(convertConfig, SegmentDefinitions.AlphabetDefs);
        KanaItems = CreateItems(convertConfig, SegmentDefinitions.KanaDefs);
        SymbolItems = CreateItems(convertConfig, SegmentDefinitions.SymbolDefs);
        EtcZenHanAsciiItems = CreateItems(convertConfig, SegmentDefinitions.EtcZenHanAsciiDefs);
        EtcBslashYenItems = CreateItems(convertConfig, SegmentDefinitions.EtcBslashYenDefs);
        EtcSpecialItems = CreateItems(convertConfig, SegmentDefinitions.EtcSpecialDefs);
        EtcMultiSpaceItems = CreateItems(convertConfig, SegmentDefinitions.EtcMultiSpaceDefs);

        foreach (var pair in convertConfig.ReplacePairs)
            AttachReplaceItem(new ReplacePairItem(pair));

        RefreshPresets();
        convertConfig.PropertyChanged += OnConfigPropertyChanged;
        RefreshSelectedPreset();
    }

    /// <summary>SegmentDefine 配列から ZenHanConvertItem のリストを生成します。</summary>
    /// <param name="config">変換設定インスタンス。</param>
    /// <param name="defs">セグメント定義配列。</param>
    /// <returns>生成された ZenHanConvertItem のリスト。</returns>
    private static IList<ZenHanConvertItem> CreateItems(ConvertConfig config, SegmentDefine[] defs)
        => [.. defs.Select(d => new ZenHanConvertItem(config, d))];

    /// <summary>全ての置換行をバリデーションし、有効な行のみを ConvertConfig に同期します。</summary>
    public void ValidateAllAndSyncToConfig()
    {
        ConvertConfig.ReplacePairs = [.. ReplaceItems
            .Where(i => i.Validate() is null)
            .Select(i => i.ToPair())];
    }

    /// <summary>指定された1行をバリデーションし、全行を同期します。</summary>
    /// <param name="item">バリデーションする置換行。</param>
    public void ValidateItemAndSync(ReplacePairItem item)
    {
        item.Validate();
        ValidateAllAndSyncToConfig();
    }

    /// <summary>プリセット読み込み後などに ReplaceItems を ConvertConfig から再構築します。</summary>
    public void ReloadReplaceItemsFromConfig()
    {
        foreach (var item in ReplaceItems)
            DetachReplaceItem(item);

        ReplaceItems.Clear();
        foreach (var pair in ConvertConfig.ReplacePairs)
        {
            AttachReplaceItem(new ReplacePairItem(pair));
        }
    }

    /// <summary>置換行を置換行一覧へ追加し、変更監視を開始します。</summary>
    /// <param name="item">追加する置換行。</param>
    private void AttachReplaceItem(ReplacePairItem item)
    {
        item.PropertyChanged += OnReplaceItemPropertyChanged;
        ReplaceItems.Add(item);
    }

    /// <summary>置換行の変更監視を解除します。</summary>
    /// <param name="item">対象の置換行。</param>
    private void DetachReplaceItem(ReplacePairItem item)
        => item.PropertyChanged -= OnReplaceItemPropertyChanged;

    /// <summary>置換行の編集内容が変化したときに、バリデーションして設定へ反映します。</summary>
    /// <param name="sender">イベントソース（変更された置換行）。</param>
    /// <param name="e">変更されたプロパティ名を含むイベントデータ。</param>
    /// <remarks>エラーメッセージ自身の変更では再検証しないことで無限ループを防ぎます。</remarks>
    private void OnReplaceItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ReplacePairItem.ErrorMessage)) return;
        if (sender is not ReplacePairItem item) return;

        ValidateItemAndSync(item);
    }

    /// <summary>置換行を追加します。</summary>
    [RelayCommand]
    private void AddReplaceRow()
    {
        AttachReplaceItem(new ReplacePairItem());
        ValidateAllAndSyncToConfig();
    }

    /// <summary>指定された置換行を削除します。</summary>
    /// <param name="item">削除する置換行。</param>
    [RelayCommand]
    private void DeleteReplaceRow(ReplacePairItem? item)
    {
        if (item is null) return;
        DetachReplaceItem(item);
        ReplaceItems.Remove(item);
        ValidateAllAndSyncToConfig();
    }

    /// <summary>設定を JSON ファイルにエクスポートします。</summary>
    /// <param name="filePath">エクスポート先のファイルパス。</param>
    public void ExportSettings(string filePath) => ConvertConfig.ExportToFile(filePath);

    /// <summary>設定を JSON ファイルからインポートします。</summary>
    /// <param name="filePath">インポート元のファイルパス。</param>
    /// <returns>成功時は null、失敗時はエラーメッセージ。</returns>
    public string? ImportSettings(string filePath)
    {
        var result = ConvertConfig.ImportFromFile(filePath);
        if (result)
        {
            ReloadReplaceItemsFromConfig();
            RefreshSelectedPreset();
            return null;
        }

        return "設定のインポートに失敗しました。ファイルが正しいJSON形式であることを確認してください。";
    }

    /// <summary>プリセット一覧を更新します。</summary>
    public void RefreshPresets()
    {
        PresetNames.Clear();
        foreach (var name in ConvertConfig.GetPresetNames())
        {
            PresetNames.Add(name);
        }
    }

    /// <summary>SelectedPresetName 変更時に呼び出され、ユーザーによるドロップダウン選択からのプリセット読み込みを実行します。</summary>
    /// <param name="value">新しく選択されたプリセット名。</param>
    /// <remarks>_isUpdatingSelection が true の場合はプログラムによる更新のため読み込みをスキップします。</remarks>
    partial void OnSelectedPresetNameChanged(string? value)
    {
        if (_isUpdatingSelection || string.IsNullOrEmpty(value)) return;

        _isUpdatingSelection = true;
        try
        {
            LoadPreset(value);
        }
        finally
        {
            _isUpdatingSelection = false;
        }
    }

    /// <summary>ConvertConfig のプロパティ変更時に呼び出されます。設定変更を検出してプリセット選択状態を更新します。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">イベントデータ。</param>
    private void OnConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isUpdatingSelection) return;
        RefreshSelectedPreset();
    }

    /// <summary>現在の設定と一致するプリセットを検索し、SelectedPresetName を更新します。</summary>
    /// <remarks>FindMatchingPreset で一致するプリセットが見つかった場合はその名前を、見つからなかった場合は null を設定します。<br/>
    /// _isUpdatingSelection フラグで OnSelectedPresetNameChanged での再読み込みを防止します。</remarks>
    private void RefreshSelectedPreset()
    {
        _isUpdatingSelection = true;
        try
        {
            SelectedPresetName = ConvertConfig.FindMatchingPreset();
        }
        finally
        {
            _isUpdatingSelection = false;
        }
    }

    /// <summary>現在の設定を指定されたプリセット名で保存します。</summary>
    /// <param name="name">プリセット名。</param>
    public void SavePreset(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        ConvertConfig.SavePreset(name);
        RefreshPresets();
        RefreshSelectedPreset();
    }

    /// <summary>プリセットを読み込みます。</summary>
    /// <param name="name">プリセット名。</param>
    public void LoadPreset(string? name)
    {
        if (string.IsNullOrEmpty(name)) return;
        _isUpdatingSelection = true;
        try
        {
            ConvertConfig.LoadPreset(name);
            ReloadReplaceItemsFromConfig();
            SelectedPresetName = name;
        }
        finally
        {
            _isUpdatingSelection = false;
        }
    }

    /// <summary>プリセットを削除します。</summary>
    /// <param name="name">プリセット名。</param>
    public void DeletePreset(string? name)
    {
        if (string.IsNullOrEmpty(name)) return;
        ConvertConfig.DeletePreset(name);
        RefreshPresets();
        if (SelectedPresetName == name)
            SelectedPresetName = null;
    }
}
