using System;
using System.Runtime.InteropServices;

namespace EsUtil.ClipboardZenHanConverter.App.WinForms.Helpers;

/// <summary>枠を持たないウィンドウ（<c>FormBorderStyle.None</c>）の移動とリサイズを補助する静的クラス。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - ヒットテスト結果（<c>WM_NCHITTEST</c> の戻り値）の定数<br/>
/// - ウィンドウ移動・リサイズのドラッグ開始<br/><br/>
/// 特徴: <br/>
/// - 枠を持たないウィンドウでは Windows が移動・リサイズの操作を提供しないため、自前で開始する<br/>
/// - 実際の操作は OS のドラッグループに委ねる（<c>WM_NCLBUTTONDOWN</c> を送る方式）。
///   自前でマウスを追跡するより、スナップ・最大化・マルチモニタ対応が OS と一致する<br/>
/// - <c>WM_NCHITTEST</c> は子コントロールがその領域を所有する場合はフォームへ届かない。
///   そのため移動はコントロールのマウスイベントから開始し、リサイズはフォームが所有する
///   外周領域（<see cref="Control.Padding"/> の領域）で <c>WM_NCHITTEST</c> に応答する<br/><br/>
/// 注意点: <br/>
/// - <see cref="BeginDrag"/> は OS のドラッグループが終わるまで戻りません
/// </remarks>
internal static partial class WindowChrome
{
    /// <summary>クライアント領域を表すヒットテスト結果。</summary>
    public const int HtClient = 1;

    /// <summary>タイトルバー（ドラッグ移動）を表すヒットテスト結果。</summary>
    public const int HtCaption = 2;

    /// <summary>左辺を表すヒットテスト結果。</summary>
    public const int HtLeft = 10;

    /// <summary>右辺を表すヒットテスト結果。</summary>
    public const int HtRight = 11;

    /// <summary>上辺を表すヒットテスト結果。</summary>
    public const int HtTop = 12;

    /// <summary>左上角を表すヒットテスト結果。</summary>
    public const int HtTopLeft = 13;

    /// <summary>右上角を表すヒットテスト結果。</summary>
    public const int HtTopRight = 14;

    /// <summary>下辺を表すヒットテスト結果。</summary>
    public const int HtBottom = 15;

    /// <summary>左下角を表すヒットテスト結果。</summary>
    public const int HtBottomLeft = 16;

    /// <summary>右下角を表すヒットテスト結果。</summary>
    public const int HtBottomRight = 17;

    /// <summary>非クライアント領域の左ボタン押下を通知するウィンドウメッセージ。</summary>
    private const uint WmNclButtonDown = 0x00A1;

    /// <summary>マウスキャプチャを解放します。</summary>
    /// <returns>解放に成功した場合は true。</returns>
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ReleaseCapture();

    /// <summary>ウィンドウメッセージを同期的に送信します。</summary>
    /// <param name="hWnd">送信先のウィンドウハンドル。</param>
    /// <param name="msg">ウィンドウメッセージ。</param>
    /// <param name="wParam">メッセージの wParam。</param>
    /// <param name="lParam">メッセージの lParam。</param>
    /// <returns>メッセージの処理結果。</returns>
    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    private static partial IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    /// <summary>指定されたヒットテスト結果でウィンドウの移動またはリサイズを開始します。</summary>
    /// <param name="handle">対象ウィンドウのハンドル。</param>
    /// <param name="hitTest">開始する操作のヒットテスト結果（<see cref="HtCaption"/> 等）。</param>
    /// <remarks>OS のドラッグループが終わるまで戻りません。<br/>
    /// 先にマウスキャプチャを解放しないと、OS がドラッグを開始できません。</remarks>
    public static void BeginDrag(IntPtr handle, int hitTest)
    {
        ReleaseCapture();
        SendMessage(handle, WmNclButtonDown, hitTest, IntPtr.Zero);
    }
}
