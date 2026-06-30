using System.Runtime.InteropServices;
using ClipboardZenHanConverter.Core.Models;
using ClipboardZenHanConverter.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;

namespace ClipboardZenHanConverter.Views;

/// <summary>アプリケーションのメインウィンドウを表示します。WinUIのWindowを継承し、タイトルバー、ナビゲーション、フレーム管理などのUI要素を管理します。</summary>
/// <remarks>
/// メインウィンドウの初期化、タイトルバーの設定、ウィンドウサイズの調整を行います。<br/>
/// <br/>
/// 【実装の詳細】<br/>
/// - DPIスケーリングを考慮したウィンドウサイズ設定を行います。<br/>
/// </remarks>
public sealed partial class MainWindow : Window
{
    /// <summary>メインウィンドウ用のViewModelです。</summary>
    public MainWindowViewModel ViewModel { get; }

    /// <summary>アプリケーション設定を保持するクラスです。</summary>
    private readonly AppSetting _appSetting;

    /// <summary>コンテンツ表示用のFrameコントロールです。</summary>
    public Frame ContentFrame => this.contentFrame;

    /// <summary>ナビゲーション用のNavigationViewコントロールです。</summary>
    public NavigationView NavigationView => navigationView;

    /// <summary>タイトルバーコントロールです。</summary>
    public TitleBar TitleBar => this.titleBar;

    /// <summary>MainWindow の新しいインスタンスを初期化します。</summary>
    /// <remarks>
    /// ViewModel を設定し、タイトルバーを拡張し、ウィンドウサイズを調整します。<br/>
    /// </remarks>
    /// <param name="viewModel">MainWindow用のViewModel</param>
    /// <param name="appSetting">アプリケーション設定クラス</param>
    public MainWindow(MainWindowViewModel viewModel, AppSetting appSetting) : base()
    {
        // WinUIコンポーネントの初期化
        this.InitializeComponent();
        this.ViewModel = viewModel;
        this._appSetting = appSetting;

        // デフォルトのタイトルバー拡張を有効に
        ExtendsContentIntoTitleBar = true;
        // WinUIのTitleBarでタイトルバーをカスタマイズ
        SetTitleBar(this.TitleBar);

        // ウィンドウがアクティブになったときに一度だけリサイズを実行
        this.Activated += MainWindow_Activated;
        // ウィンドウが閉じるときにサイズを保存
        this.Closed += MainWindow_Closed;
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        this.Activated -= MainWindow_Activated;

        // DPIスケールを取得
        double dpiScale = GetWindowDpiScale(this);
        // ウィンドウサイズを設定ファイル（またはデフォルト値）とDPIスケールに基づいて設定
        AppWindow?.Resize(new Windows.Graphics.SizeInt32(
                (int)(_appSetting.WindowWidth * dpiScale),
                (int)(_appSetting.WindowHeight * dpiScale)));
    }

    /// <summary>ウィンドウが閉じられる時に呼び出されます。</summary>
    /// <remarks>
    /// 現在のウィンドウサイズをDPIスケールを考慮して論理ピクセルで保存します。<br/>
    /// </remarks>
    /// <param name="sender">Windowオブジェクト</param>
    /// <param name="args">イベント引数</param>
    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        if (AppWindow == null) return;

        // 現在のDPIスケールを取得
        double dpiScale = GetWindowDpiScale(this);

        // 自動保存を抑制（プロパティ変更による Debounce 予約を防止）
        bool wasAutoSave = _appSetting.IsAutoSave;
        _appSetting.IsAutoSave = false;

        // 現在の物理サイズから論理サイズを計算して保存
        _appSetting.WindowWidth = AppWindow.Size.Width / dpiScale;
        _appSetting.WindowHeight = AppWindow.Size.Height / dpiScale;

        // 自動保存を元に戻す
        _appSetting.IsAutoSave = wasAutoSave;

        // プロパティ変更で予約された不要な Debounce をキャンセル
        _appSetting.CancelPendingSave();

        // 終了時に確実に保存を実行
        _appSetting.SaveToJsonFile(_appSetting.AutoSaveFileName);
    }


    /// <summary>タイトルバーの「戻る」ボタンが押された時に呼び出されます。</summary>

    /// <remarks>
    /// ContentFrame が戻れる場合に GoBack を実行します。<br/>
    /// </remarks>
    /// <param name="sender">TitleBarコントロール</param>
    /// <param name="args">イベント引数</param>
    private void TitleBar_BackRequested(TitleBar sender, object args)
    {
        // フレームが戻れる場合に前のページに戻る
        if (this.ContentFrame.CanGoBack)
            this.ContentFrame.GoBack();
    }

    /// <summary>タイトルバーのペイン切り替えボタンが押された時に呼び出されます。</summary>
    /// <remarks>
    /// NavigationView のペインを開閉します。<br/>
    /// </remarks>
    /// <param name="sender">TitleBarコントロール</param>
    /// <param name="args">イベント引数</param>
    private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
    {
        // ナビゲーションペインの開閉をトグル
        this.NavigationView.IsPaneOpen = !this.NavigationView.IsPaneOpen;
    }

    /// <summary>指定したウィンドウハンドルのDPI値を取得します。</summary>
    /// <remarks>
    /// Windows API User32.dll の GetDpiForWindow を呼び出します。<br/>
    /// </remarks>
    /// <param name="hwnd">DPI値を取得するウィンドウのハンドル</param>
    /// <returns>ウィンドウのDPI値</returns>
    [LibraryImport("User32.dll")]
    private static partial int GetDpiForWindow(nint hwnd);

    /// <summary>指定した Window のDPIスケールを取得します。</summary>
    /// <remarks>
    /// ウィンドウのハンドルからDPI値を取得し、デフォルトDPIで割ってスケールを計算します。<br/>
    /// デフォルト96DPIを基準としています。<br/>
    /// </remarks>
    /// <param name="window">DPIスケールを取得する対象のウィンドウ</param>
    /// <returns>DPIスケール (例: 1.0, 1.25など)</returns>
    public static double GetWindowDpiScale(Window window)
    {
        // ウィンドウハンドルを取得
        nint windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        if (windowHandle == 0) return 1.0;
        const double DefaultPixelsPerInch = 96D;
        // DPIスケールを計算して返す
        return GetDpiForWindow(windowHandle) / DefaultPixelsPerInch;
    }
}
