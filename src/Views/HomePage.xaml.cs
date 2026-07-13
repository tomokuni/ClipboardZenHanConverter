using ClipboardZenHanConverter.Core.Interfaces;
using ClipboardZenHanConverter.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ClipboardZenHanConverter.Views;

/// <summary>ホーム画面を表示するページクラスです。</summary>
/// <remarks>
/// HomeViewModel を使用してデータをバインドします。<br/>
/// <br/>
/// 【実装の詳細】<br/>
/// - DI で HomeViewModel を注入します。<br/>
/// </remarks>
public sealed partial class HomePage : Page, INavigationAware
{
    /// <summary>ホームページ用のViewModelです。</summary>
    public HomeViewModel ViewModel { get; }

    /// <summary>HomePage の新しいインスタンスを初期化します。</summary>
    /// <remarks>
    /// ViewModel を設定し、コンポーネントを初期化します。<br/>
    /// </remarks>
    /// <param name="viewModel">HomePage用のViewModel</param>
    public HomePage(HomeViewModel viewModel) : base()
    {
        ViewModel = viewModel;
        // WinUIコンポーネントの初期化
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
}
