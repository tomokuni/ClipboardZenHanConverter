using Avalonia.Controls;

namespace ClipboardZenHanConverter.App.AvaloniaUI.Views;

/// <summary>プリセットの保存・削除を行うダイアログです。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - プリセット名の入力とリアルタイム検証<br/>
/// - 保存 / 削除 / 閉じる<br/><br/>
/// 特徴: <br/>
/// - 検証と操作は <see cref="PresetEditDialogViewModel"/> が保持し、View は表示のみを担う
/// </remarks>
public partial class PresetEditDialog : Window
{
    /// <summary>PresetEditDialog の新しいインスタンスを初期化します。</summary>
    public PresetEditDialog()
    {
        InitializeComponent();
    }

    /// <summary>ダイアログの ViewModel を設定し、初期フォーカスを設定します。</summary>
    /// <param name="viewModel">プリセット編集の ViewModel。</param>
    public void Initialize(PresetEditDialogViewModel viewModel)
    {
        viewModel.SetCloseAction(() => Close());
        DataContext = viewModel;

        Opened += (_, _) => PresetNameBox.Focus();
    }
}
