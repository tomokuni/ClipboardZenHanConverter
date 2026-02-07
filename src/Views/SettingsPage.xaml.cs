using ClipboardZenHanConverter.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ClipboardZenHanConverter.Views;

/// <summary>設定画面を表示するページクラスです。</summary>
/// <remarks>
/// SettingsViewModel を使用してデータをバインドします。<br/>
/// <br/>
/// 【実装の詳細】<br/>
/// - DI で SettingsViewModel を注入します。<br/>
/// </remarks>
public sealed partial class SettingsPage : Page
{
    /// <summary>設定ページ用のViewModelです。</summary>
    public SettingsViewModel ViewModel { get; }

    /// <summary>SettingsPage の新しいインスタンスを初期化します。</summary>
    /// <param name="viewModel">SettingsPage用のViewModel</param>
    public SettingsPage(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        // WinUIコンポーネントの初期化を最優先で行う
        this.InitializeComponent();
    }
}
