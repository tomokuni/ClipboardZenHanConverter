using System;
using System.IO;
using System.Linq;
using ClipboardZenHanConverter.Core.Interfaces;
using ClipboardZenHanConverter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace ClipboardZenHanConverter.Views;

/// <summary>設定画面を表示するページクラスです。</summary>
public sealed partial class SettingsPage : Page, INavigationAware
{
    /// <summary>設定ページ用のViewModelです。</summary>
    public SettingsViewModel ViewModel { get; }

    public SettingsPage(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
    }

    /// <summary>このページに遷移してきたときに呼び出されます。</summary>
    /// <param name="parameter">ナビゲーションパラメータ</param>
    public void OnNavigatedTo(object? parameter)
    {
    }

    /// <summary>このページから別のページに遷移するときに呼び出されます。</summary>
    public void OnNavigatingFrom()
    {
    }

    /// <summary>置換行を編集します。</summary>
    private async void OnEditReplaceRowClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ReplacePairItem item }) return;

        var searchBox = new TextBox
        {
            Text = item.Search,
            PlaceholderText = "検索文字列",
            MinWidth = 300,
            Margin = new Thickness(0, 0, 0, 8),
        };
        var replaceBox = new TextBox
        {
            Text = item.Replace,
            PlaceholderText = "置換文字列",
            MinWidth = 300,
            Margin = new Thickness(0, 0, 0, 8),
        };
        var regexCheck = new CheckBox
        {
            IsChecked = item.IsRegex,
            Content = "正規表現",
            Margin = new Thickness(0, 0, 0, 8),
        };

        var panel = new StackPanel();
        panel.Children.Add(new TextBlock { Text = "検索文字列", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        panel.Children.Add(searchBox);
        panel.Children.Add(new TextBlock { Text = "置換文字列", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        panel.Children.Add(replaceBox);
        panel.Children.Add(regexCheck);

        var dialog = new ContentDialog
        {
            Title = "置換設定の編集",
            Content = panel,
            PrimaryButtonText = "保存",
            CloseButtonText = "キャンセル",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot,
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            item.Search = searchBox.Text;
            item.Replace = replaceBox.Text;
            item.IsRegex = regexCheck.IsChecked == true;
            ViewModel.ValidateAllAndSyncToConfig();
            // DataGrid を強制リフレッシュ
            var src = ReplaceDataGrid.ItemsSource;
            ReplaceDataGrid.ItemsSource = null;
            ReplaceDataGrid.ItemsSource = src;
        }
    }

    /// <summary>置換行を削除します。</summary>
    private void OnDeleteReplaceRowClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: ReplacePairItem item })
        {
            ViewModel.DeleteReplaceRowCommand.Execute(item);
        }
    }

    /// <summary>新しい置換行を追加します。</summary>
    private void OnAddReplaceRowClick(object sender, RoutedEventArgs e)
    {
        ViewModel.AddReplaceRowCommand.Execute(null);
    }

    /// <summary>設定をエクスポートします。</summary>
    private async void OnExportSettingsClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = "ClipboardZenHanConverter_Settings.json",
        };
        picker.FileTypeChoices.Add("JSON ファイル", [".json"]);

        InitializeWithWindow.Initialize(picker, GetWindowHandle());

        var file = await picker.PickSaveFileAsync();
        if (file is not null)
        {
            ViewModel.ExportSettings(file.Path);
        }
    }

    /// <summary>設定をインポートします。</summary>
    private async void OnImportSettingsClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        };
        picker.FileTypeFilter.Add(".json");

        InitializeWithWindow.Initialize(picker, GetWindowHandle());

        var file = await picker.PickSingleFileAsync();
        if (file is not null)
        {
            var error = ViewModel.ImportSettings(file.Path);
            if (error is not null)
            {
                var dialog = new ContentDialog
                {
                    Title = "インポートエラー",
                    Content = error,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot,
                };
                await dialog.ShowAsync();
            }
        }
    }

    /// <summary>選択されたプリセットを読み込みます。</summary>
    private void OnLoadPresetClick(object sender, RoutedEventArgs e)
    {
        if (PresetComboBox.SelectedItem is string name)
        {
            ViewModel.LoadPresetCommand.Execute(name);
        }
    }

    /// <summary>選択されたプリセットを削除します。</summary>
    private void OnDeletePresetClick(object sender, RoutedEventArgs e)
    {
        if (PresetComboBox.SelectedItem is string name)
        {
            ViewModel.DeletePresetCommand.Execute(name);
        }
    }

    /// <summary>例外をログファイルに記録します。</summary>
    private static void LogException(Exception ex, string source)
    {
        try
        {
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ClipboardZenHanConverter",
                "crash.log");
            var msg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {source}\n{ex}\n\n";
            File.AppendAllText(logPath, msg);
        }
        catch
        {
            // ログ記録の失敗は無視
        }
    }

    private nint GetWindowHandle()
    {
        var mainWindow = App.GetService<MainWindow>();
        return WindowNative.GetWindowHandle(mainWindow);
    }
}
