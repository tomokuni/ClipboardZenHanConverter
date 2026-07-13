using System.Runtime.InteropServices;
using ClipboardZenHanConverter.Core.Models;
using ClipboardZenHanConverter.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;

namespace ClipboardZenHanConverter.App.Views;

/// <summary>アプリケーションのメインウィンドウを表します。</summary>
/// <remarks>NavigationView と TitleBar を備え、ページ遷移のコンテナとして機能します。<br/>
/// 特徴: <br/>
/// - アクティブ化時に AppSetting のウィンドウサイズを復元<br/>
/// - クローズ時に現在のウィンドウサイズを AppSetting に保存<br/>
/// - MicaBackdrop による高速な背景レンダリング<br/>
/// - タイトルバーの拡張（ExtendsContentIntoTitleBar）</remarks>
public sealed partial class MainWindow : Window
{
    /// <summary>メインウィンドウの ViewModel を取得します。</summary>
    public MainWindowViewModel ViewModel { get; }
    /// <summary>ウィンドウサイズ保存用のアプリケーション設定。</summary>
    private readonly AppSetting _appSetting;

    /// <summary>ページコンテンツを表示する Grid。XAML の x:Name="contentFrame" にバインド。</summary>
    public Grid ContentFrame => this.contentFrame;
    /// <summary>ナビゲーションメニュー。XAML の x:Name="navigationView" にバインド。</summary>
    public NavigationView NavigationView => navigationView;
    /// <summary>カスタムタイトルバー。XAML の x:Name="titleBar" にバインド。</summary>
    public TitleBar TitleBar => this.titleBar;

    /// <summary>MainWindow の新しいインスタンスを初期化します。</summary>
    /// <param name="viewModel">メインウィンドウの ViewModel</param>
    /// <param name="appSetting">ウィンドウサイズ保存用の AppSetting</param>
    public MainWindow(MainWindowViewModel viewModel, AppSetting appSetting)
    {
        this.InitializeComponent();
        this.ViewModel = viewModel;
        this._appSetting = appSetting;

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(this.TitleBar);

        this.Activated += MainWindow_Activated;
        this.Closed += MainWindow_Closed;
    }

    /// <summary>ウィンドウアクティブ化時に AppSetting からウィンドウサイズを復元します。</summary>
    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        this.Activated -= MainWindow_Activated;

        double dpiScale = GetWindowDpiScale(this);
        AppWindow?.Resize(new SizeInt32(
                (int)(_appSetting.WindowWidth * dpiScale),
                (int)(_appSetting.WindowHeight * dpiScale)));
    }

    /// <summary>ウィンドウクローズ時に現在のサイズを AppSetting に保存します。</summary>
    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        if (AppWindow == null) return;
        double dpiScale = GetWindowDpiScale(this);
        _appSetting.WindowWidth = AppWindow.Size.Width / dpiScale;
        _appSetting.WindowHeight = AppWindow.Size.Height / dpiScale;
        _appSetting.SaveToJsonFile(_appSetting.AutoSaveFileName);
    }

    /// <summary>タイトルバーのパネルトグルリクエストを処理します。</summary>
    private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
    {
        this.NavigationView.IsPaneOpen = !this.NavigationView.IsPaneOpen;
    }

    /// <summary>指定されたウィンドウハンドルの DPI 値を取得します（Win32 API）。</summary>
    /// <param name="hwnd">ウィンドウハンドル</param>
    /// <returns>DPI 値（例: 96, 120, 144）</returns>
    [LibraryImport("User32.dll")]
    private static partial int GetDpiForWindow(nint hwnd);

    /// <summary>指定されたウィンドウの DPI スケールを取得します。</summary>
    /// <param name="window">DPI スケールを取得するウィンドウ</param>
    /// <returns>DPI スケール値（例: 1.0, 1.25, 1.5）</returns>
    public static double GetWindowDpiScale(Window window)
    {
        nint windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        if (windowHandle == 0) return 1.0;
        return GetDpiForWindow(windowHandle) / 96D;
    }
}
