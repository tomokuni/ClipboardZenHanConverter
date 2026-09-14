using EsUtil.ClipboardZenHanConverter.Core.Models;
using EsUtil.ClipboardZenHanConverter.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace EsUtil.ClipboardZenHanConverter.App.WinUI.Views;

/// <summary>設定画面を表示するページクラスです。</summary>
/// <remarks>8つの変換カテゴリの選択、ユーザー定義置換ルールの編集、プリセット管理、<br/>
/// 設定のエクスポート/インポートを提供します。<br/>
/// 各操作は ContentDialog や FilePicker 等の WinUI 標準コントロールを使用します。</remarks>
public sealed partial class SettingsPage : Page
{
    /// <summary>設定ページ用の ViewModel を取得します。</summary>
    public SettingsViewModel ViewModel { get; }

    /// <summary>SettingsPage の新しいインスタンスを初期化します。</summary>
    /// <param name="viewModel">設定ページ用の ViewModel</param>
    public SettingsPage(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
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

    /// <summary>プリセット編集ダイアログを表示します。</summary>
    /// <remarks>編集可能なコンボボックスでプリセット名を選択/入力し、保存・削除・閉じるの操作を行います。<br/>
    /// バリデーションは選択変更時、フォーカス喪失時、およびコンボボックス内のテキスト内容が変化した時に行われます。<br/>
    /// 組込みプリセット名が入力された場合は「組込みプリセットです。」と表示し保存/削除が無効化されます。<br/>
    /// ファイル名に使用できない文字が含まれる場合は「使用できない文字が含まれます。」と表示し保存が無効化されます。<br/>
    /// 半角小文字に変換した際に built-in/builtin が含まれる場合は「built-in または builtin は使用できません。」と表示し保存が無効化されます。<br/>
    /// 既存のユーザープリセット名が入力された場合は「既に存在します。」と表示しますが保存/削除は可能です。</remarks>
    private async void OnEditPresetClick(object sender, RoutedEventArgs e)
    {
        var comboBox = new ComboBox
        {
            IsEditable = true,
            PlaceholderText = "プリセット名を選択または入力",
            MinWidth = 300,
            Margin = new Thickness(0, 0, 0, 8),
        };

        // 組込みプリセットを除いたユーザープリセットのみをリスト
        var userPresets = ViewModel.PresetNames
            .Where(n => !ConvertConfig.IsBuiltInPreset(n))
            .ToList();
        foreach (var name in userPresets)
            comboBox.Items.Add(name);

        // バリデーションメッセージ
        var validationText = new TextBlock
        {
            Text = string.Empty,
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
            Margin = new Thickness(0, 0, 0, 8),
            Visibility = Visibility.Collapsed,
        };

        var saveButton = new Button { Content = "保存", Margin = new Thickness(0, 0, 8, 0) };
        var deleteButton = new Button { Content = "削除", Margin = new Thickness(0, 0, 8, 0) };
        var closeButton = new Button { Content = "閉じる" };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        buttonPanel.Children.Add(saveButton);
        buttonPanel.Children.Add(deleteButton);
        buttonPanel.Children.Add(closeButton);

        var panel = new StackPanel();
        panel.Children.Add(comboBox);
        panel.Children.Add(validationText);
        panel.Children.Add(buttonPanel);

        // バリデーションを実行する共通処理
        void Validate() => ValidatePresetName(comboBox, validationText, saveButton, deleteButton, userPresets);

        // 選択変更時のバリデーション
        comboBox.SelectionChanged += (_, _) => Validate();

        // フォーカス喪失時のバリデーション
        comboBox.LostFocus += (_, _) => Validate();

        // 内容が変化したときのリアルタイムバリデーション
        comboBox.Loaded += (_, _) =>
        {
            if (FindInnerTextBox(comboBox) is { } innerTextBox)
                innerTextBox.TextChanged += (_, _) => Validate();
        };

        // 初期状態：何も選択されていないので保存/削除は無効
        saveButton.IsEnabled = false;
        deleteButton.IsEnabled = false;

        var dialog = new ContentDialog
        {
            Title = "プリセット編集",
            Content = panel,
            XamlRoot = this.XamlRoot,
        };

        // 閉じるボタン
        closeButton.Click += (_, _) => dialog.Hide();

        // 保存ボタン
        saveButton.Click += (_, _) =>
        {
            var name = comboBox.Text?.Trim();
            if (string.IsNullOrEmpty(name)) return;

            dialog.Hide();
            ViewModel.SavePreset(name);
        };

        // 削除ボタン
        deleteButton.Click += (_, _) =>
        {
            var name = comboBox.Text?.Trim();
            if (string.IsNullOrEmpty(name)) return;

            dialog.Hide();
            ViewModel.DeletePreset(name);
        };

        await dialog.ShowAsync();

        // ダイアログ表示中に変更があった場合に備えてプリセット一覧を更新
        ViewModel.RefreshPresets();
    }

    /// <summary>プリセット名のバリデーションを行い、UI の表示とボタン状態を更新します。</summary>
    /// <param name="comboBox">編集可能な ComboBox</param>
    /// <param name="validationText">バリデーションメッセージ表示用 TextBlock</param>
    /// <param name="saveButton">保存ボタン</param>
    /// <param name="deleteButton">削除ボタン</param>
    /// <param name="userPresets">ユーザープリセット名の一覧</param>
    private static void ValidatePresetName(ComboBox comboBox, TextBlock validationText,
        Button saveButton, Button deleteButton, List<string> userPresets)
    {
        var text = comboBox.Text?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            validationText.Text = string.Empty;
            validationText.Visibility = Visibility.Collapsed;
            saveButton.IsEnabled = false;
            deleteButton.IsEnabled = false;
        }
        else if (ConvertConfig.IsBuiltInPreset(text))
        {
            validationText.Text = "組込みプリセットです。";
            validationText.Visibility = Visibility.Visible;
            saveButton.IsEnabled = false;
            deleteButton.IsEnabled = false;
        }
        else if (ContainsInvalidFileNameChars(text))
        {
            validationText.Text = "使用できない文字が含まれます。";
            validationText.Visibility = Visibility.Visible;
            saveButton.IsEnabled = false;
            deleteButton.IsEnabled = userPresets.Contains(text, StringComparer.Ordinal);
        }
        else if (ContainsBuiltInKeyword(text))
        {
            validationText.Text = "built-in または builtin は使用できません。";
            validationText.Visibility = Visibility.Visible;
            saveButton.IsEnabled = false;
            deleteButton.IsEnabled = userPresets.Contains(text, StringComparer.Ordinal);
        }
        else if (userPresets.Contains(text, StringComparer.Ordinal))
        {
            validationText.Text = "既に存在します。";
            validationText.Visibility = Visibility.Visible;
            saveButton.IsEnabled = true;
            deleteButton.IsEnabled = true;
        }
        else
        {
            validationText.Text = string.Empty;
            validationText.Visibility = Visibility.Collapsed;
            saveButton.IsEnabled = true;
            deleteButton.IsEnabled = false;
        }
    }

    /// <summary>ファイル名に使用できない文字が含まれているかを判定します。</summary>
    /// <param name="name">チェックする文字列</param>
    /// <returns>使用できない文字が含まれる場合は true</returns>
    /// <remarks>Windows のファイル名に使用できない文字（\/:*?"&lt;&gt;|）をチェックします。</remarks>
    internal static bool ContainsInvalidFileNameChars(string name)
        => name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0;

    /// <summary>文字列を半角小文字に変換した際に "built-in" または "builtin" が含まれるかを判定します。</summary>
    /// <param name="name">チェックする文字列</param>
    /// <returns>含まれる場合は true</returns>
    /// <remarks>NFKC 正規化で全角英数字を半角に変換し、ToLowerInvariant で小文字化した上で判定します。</remarks>
    internal static bool ContainsBuiltInKeyword(string name)
    {
        var normalized = name.Normalize(NormalizationForm.FormKC).ToLowerInvariant();
        return normalized.Contains("built-in") || normalized.Contains("builtin");
    }

    /// <summary>ComboBox のコントロールテンプレート内部にある TextBox を検索します。</summary>
    /// <param name="comboBox">編集可能な ComboBox</param>
    /// <returns>内部の TextBox。見つからない場合は null。</returns>
    /// <remarks>VisualTreeHelper を使用してビジュアルツリーを走査し、ComboBox のテンプレート内の TextBox を取得します。<br/>
    /// Loaded イベント以降でないとテンプレートが適用されていないため、Loaded 後に呼び出す必要があります。</remarks>
    private static TextBox? FindInnerTextBox(DependencyObject comboBox)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(comboBox); i++)
        {
            var child = VisualTreeHelper.GetChild(comboBox, i);
            if (child is TextBox tb) return tb;
            var found = FindInnerTextBox(child);
            if (found is not null) return found;
        }
        return null;
    }

    /// <summary>プリセット選択ドロップダウンの選択が変更された時に呼び出されます。</summary>
    /// <remarks>OneWay バインディングのため、ユーザー操作による選択変更を ViewModel に通知します。</remarks>
    private void OnPresetComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.FirstOrDefault() is string name)
        {
            ViewModel.LoadPreset(name);
        }
    }

    /// <summary>メインウィンドウのウィンドウハンドルを取得します。FilePicker の表示に必要です。</summary>
    /// <returns>Win32 ウィンドウハンドル（HWND）</returns>
    private static nint GetWindowHandle()
    {
        var mainWindow = App.GetService<MainWindow>();
        return WindowNative.GetWindowHandle(mainWindow);
    }
}
