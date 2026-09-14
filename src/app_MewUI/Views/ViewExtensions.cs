using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using ClipboardZenHanConverter.App.MewUI.ViewModels;
using System.Collections.Specialized;

namespace ClipboardZenHanConverter.App.MewUI.Views;


/// <summary>ビューから ViewModel への選択状態・置換行のバインディングを支援する拡張クラス。</summary>
internal static class ViewExtensions
{
    /// <summary>ComboBox の選択項目をプリセット選択プロパティ（string?）に双方向に同期します。</summary>
    /// <param name="comboBox">バインド対象のコンボボックス。</param>
    /// <param name="viewModel">設定画面の ViewModel。</param>
    /// <returns>設定済みコンボボックス。</returns>
    public static ComboBox BindSelectedPreset(this ComboBox comboBox, SettingsViewModel viewModel)
    {
        comboBox.ItemsSource = ItemsView.Create([.. viewModel.PresetNames]);
        comboBox.SelectedIndex = FindIndex(viewModel.PresetNames, viewModel.SelectedPresetName);

        // ComboBox → ViewModel（ユーザーによる選択）
        comboBox.OnSelectionChanged(_ =>
        {
            var selected = comboBox.SelectedText;
            viewModel.SelectedPresetName = string.IsNullOrEmpty(selected) ? null : selected;
        });

        // プリセット一覧の変更を ComboBox へ反映
        viewModel.PresetNames.CollectionChanged += (_, _) =>
        {
            comboBox.ItemsSource = ItemsView.Create([.. viewModel.PresetNames]);
            comboBox.SelectedIndex = FindIndex(viewModel.PresetNames, viewModel.SelectedPresetName);
        };

        // ViewModel → ComboBox（プリセット連動/一致検出など）
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SettingsViewModel.SelectedPresetName) ||
                e.PropertyName == nameof(SettingsViewModel.PresetNames))
                comboBox.SelectedIndex = FindIndex(viewModel.PresetNames, viewModel.SelectedPresetName);
        };

        return comboBox;
    }

    /// <summary>指定されたプリセット名に対応する ComboBox インデックスを取得します。</summary>
    private static int FindIndex(IList<string> names, string? selected)
    {
        if (selected is null) return -1;
        for (var i = 0; i < names.Count; i++)
        {
            if (names[i] == selected) return i;
        }
        return -1;
    }

    /// <summary>置換ルール一覧の行を再構築します。</summary>
    /// <param name="viewModel">設定画面の ViewModel。</param>
    /// <param name="panel">行を追加するパネル。</param>
    internal static void RebuildReplaceRows(StackPanel panel, SettingsViewModel viewModel)
    {
        panel.Clear();
        foreach (var item in viewModel.ReplaceItems)
        {
            panel.Add(BuildReplaceRow(viewModel, item));
        }
    }

    /// <summary>単一の置換ルール行を構築します。</summary>
    /// <param name="viewModel">設定画面の ViewModel。</param>
    /// <param name="item">置換ルール行。</param>
    /// <returns>置換ルール行の要素。</returns>
    internal static Element BuildReplaceRow(SettingsViewModel viewModel, ReplacePairItem item)
    {
        var search = new ObservableValue<string>(item.Search);
        var replace = new ObservableValue<string>(item.Replace);
        var isRegex = new ObservableValue<bool>(item.IsRegex, v => v);

        return new Grid()
            .Columns("*,*,Auto,Auto")
            .Spacing(8)
            .Children(
                new TextBox()
                    .BindText(search)
                    .OnTextChanged(value => item.Search = value),
                new TextBox()
                    .Column(1)
                    .BindText(replace)
                    .OnTextChanged(value => item.Replace = value),
                new CheckBox()
                    .Column(2)
                    .Content("正規表現")
                    .BindIsChecked(isRegex)
                    .OnCheckedChanged(value => item.IsRegex = value),
                new Button()
                    .Column(3)
                    .Content("削除")
                    .OnClick(() => viewModel.DeleteReplaceRowCommand.Execute(item))
            );
    }
}
