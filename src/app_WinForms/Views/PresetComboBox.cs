using EsUtil.ClipboardZenHanConverter.Presentation.ViewModels;
using System;
using System.Linq;
using System.Windows.Forms;

namespace EsUtil.ClipboardZenHanConverter.App.WinForms.Views;

/// <summary>プリセット選択のドロップダウンを提供します。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - プリセット一覧の表示と選択<br/>
/// - <see cref="SettingsViewModel.SelectedPresetName"/> との双方向同期<br/>
/// - プリセットの追加・削除に追従する一覧の再構築<br/><br/>
/// 特徴: <br/>
/// - タイトルバーと設定画面が同一の実装を共有する（プリセット選択の単一所有元は SettingsViewModel）<br/>
/// - WinForms のバインディングは <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/> の
///   変更通知に追従しないため、コレクション変更とプロパティ変更を購読して表示を更新する
/// </remarks>
internal sealed class PresetComboBox : ComboBox
{
    /// <summary>プリセット選択の単一所有元。</summary>
    private readonly SettingsViewModel _viewModel;

    /// <summary>表示の更新中フラグ。更新に伴う選択変更イベントの再入を防ぎます。</summary>
    private bool _isUpdating;

    /// <summary>PresetComboBox の新しいインスタンスを初期化します。</summary>
    /// <param name="viewModel">設定画面の ViewModel。プリセット選択の単一所有元。</param>
    public PresetComboBox(SettingsViewModel viewModel)
    {
        _viewModel = viewModel;
        DropDownStyle = ComboBoxStyle.DropDownList;

        _viewModel.PresetNames.CollectionChanged += OnPresetNamesChanged;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        SelectedIndexChanged += OnSelectedIndexChanged;

        SyncItemsFromViewModel();
    }

    /// <summary>購読しているイベントを解除します。</summary>
    /// <param name="disposing">マネージリソースを破棄する場合は true。</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _viewModel.PresetNames.CollectionChanged -= OnPresetNamesChanged;
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            SelectedIndexChanged -= OnSelectedIndexChanged;
        }

        base.Dispose(disposing);
    }

    /// <summary>プリセット一覧の変更時に表示を再構築します。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">イベントデータ。</param>
    private void OnPresetNamesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        => SyncItemsFromViewModel();

    /// <summary>ViewModel のプロパティ変更時に選択状態を同期します。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">変更されたプロパティ名を含むイベントデータ。</param>
    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SettingsViewModel.SelectedPresetName)) return;
        SyncItemsFromViewModel();
    }

    /// <summary>ユーザー操作による選択変更を ViewModel へ反映します。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">イベントデータ。</param>
    private void OnSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_isUpdating) return;
        _viewModel.SelectedPresetName = SelectedItem as string;
    }

    /// <summary>プリセット一覧と選択状態を ViewModel に合わせて更新します。</summary>
    /// <remarks>一覧が同一の場合は項目を作り直さず、選択状態のみを更新します。</remarks>
    private void SyncItemsFromViewModel()
    {
        _isUpdating = true;
        try
        {
            var names = _viewModel.PresetNames.ToArray();
            if (!names.SequenceEqual(Items.Cast<string>()))
            {
                BeginUpdate();
                Items.Clear();
                Items.AddRange(names);
                EndUpdate();
            }

            var selected = _viewModel.SelectedPresetName;
            SelectedItem = selected is not null && names.Contains(selected, StringComparer.Ordinal)
                ? selected
                : null;
            SelectedIndex = SelectedItem is null ? -1 : SelectedIndex;
        }
        finally
        {
            _isUpdating = false;
        }
    }
}
