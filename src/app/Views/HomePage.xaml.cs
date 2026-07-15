using ClipboardZenHanConverter.App.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ClipboardZenHanConverter.App.Views;

/// <summary>ホーム画面を表示するページクラスです。</summary>
public sealed partial class HomePage : Page
{
    /// <summary>ホームページ用のViewModelです。</summary>
    public HomeViewModel ViewModel { get; }

    /// <summary>HomePage の新しいインスタンスを初期化します。</summary>
    /// <param name="viewModel">ホームページ用の ViewModel</param>
    public HomePage(HomeViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
    }

    /// <summary>ホーム画面のプリセット選択が変更された時に呼び出されます。</summary>
    private void OnHomePresetComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.FirstOrDefault() is string name)
        {
            ViewModel.SelectedPresetName = name;
        }
    }
}
