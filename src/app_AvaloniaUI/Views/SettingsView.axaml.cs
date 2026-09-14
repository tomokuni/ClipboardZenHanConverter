using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using EsUtil.ClipboardZenHanConverter.Presentation.ViewModels;

namespace EsUtil.ClipboardZenHanConverter.App.AvaloniaUI.Views;

/// <summary>設定画面を表示するビューです。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - 8 カテゴリの変換設定（セグメント選択）<br/>
/// - 文字列の置換（追加・編集・削除）<br/>
/// - プリセットの選択・編集、設定の JSON エクスポート/インポート<br/><br/>
/// 特徴: <br/>
/// - ファイルの選択は Avalonia の StorageProvider を使い、パスの解決のみを View が担う<br/>
/// - 設定の読み書き自体は ViewModel へ委譲（UI 非依存のロジックとしてテスト可能）
/// </remarks>
public partial class SettingsView : UserControl
{
    /// <summary>設定画面の ViewModel を取得します。</summary>
    public SettingsViewModel ViewModel { get; }

    /// <summary>SettingsView の新しいインスタンスを初期化します。</summary>
    /// <param name="viewModel">設定画面の ViewModel。</param>
    public SettingsView(SettingsViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
    }

    /// <summary>プリセット編集ダイアログを表示します。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">イベントデータ。</param>
    private async void OnEditPresetClick(object? sender, RoutedEventArgs e)
    {
        var owner = TopLevel.GetTopLevel(this) as Window;
        if (owner is null) return;

        var dialog = new PresetEditDialog();
        dialog.Initialize(new PresetEditDialogViewModel(ViewModel));
        await dialog.ShowDialog(owner);
    }

    /// <summary>現在の設定を JSON ファイルへエクスポートします。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">イベントデータ。</param>
    private async void OnExportSettingsClick(object? sender, RoutedEventArgs e)
    {
        var storage = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (storage is null) return;

        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "設定のエクスポート",
            SuggestedFileName = "ClipboardZenHanConverter_Settings.json",
            FileTypeChoices = [JsonFileType],
        });

        var path = file?.TryGetLocalPath();
        if (path is null) return;

        ViewModel.ExportSettings(path);
    }

    /// <summary>JSON ファイルから設定をインポートします。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">イベントデータ。</param>
    private async void OnImportSettingsClick(object? sender, RoutedEventArgs e)
    {
        var storage = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (storage is null) return;

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "設定のインポート",
            AllowMultiple = false,
            FileTypeFilter = [JsonFileType],
        });

        var path = files.Count > 0 ? files[0].TryGetLocalPath() : null;
        if (path is null) return;

        var error = ViewModel.ImportSettings(path);
        if (error is not null)
            await MessageDialog.ShowAsync(this, error);
    }

    /// <summary>ファイル選択に使用する JSON のファイル種別。</summary>
    private static FilePickerFileType JsonFileType => new("JSON ファイル")
    {
        Patterns = ["*.json"],
    };
}
