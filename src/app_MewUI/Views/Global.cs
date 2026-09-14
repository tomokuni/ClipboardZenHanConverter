using Aprillz.MewUI;
using ClipboardZenHanConverter.App.MewUI.ViewModels;

namespace ClipboardZenHanConverter.App.MewUI.Views;

/// <summary>アプリケーション全体で共有されるグローバル状態を提供します。</summary>
/// <remarks>
/// MewUI の Window インスタンスをビューから参照するための軽量なグローバルアクセサです。<br/>
/// ダイアログやファイルピッカーのオーナーウィンドウとして使用します。
/// </remarks>
internal static class Global
{
    /// <summary>メインウィンドウを取得または設定します。</summary>
    /// <remarks>Program.cs でメインウィンドウを表示する際に設定されます。</remarks>
    public static Window? MainWindow { get; set; }
}
