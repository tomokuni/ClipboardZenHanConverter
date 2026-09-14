using ClipboardZenHanConverter.Core.Geometry;
using ClipboardZenHanConverter.Core.Helpers;

namespace ClipboardZenHanConverter.Core.Native;

/// <summary>仮想画面（全モニタの外接矩形）に基づくウィンドウ位置の補正を提供する静的クラス。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - 仮想画面の外接矩形を DIP で取得<br/>
/// - 保存されたウィンドウ位置を表示領域内へ補正<br/>
/// - DIP と物理ピクセルの換算（丸め規則を一元管理）<br/><br/>
/// 特徴: <br/>
/// - Win32 API（<see cref="Win32Display"/>）と補正ロジック（<see cref="WindowPlacement"/>）の組み合わせを
///   単一所有し、各 UI のウィンドウ復元処理から重複を排除する<br/>
/// - 補正ロジック自体は <see cref="WindowPlacement"/> が純粋関数として所有し、本クラスは Win32 依存部分のみを担う<br/><br/>
/// 注意点: <br/>
/// - <see cref="Win32Display"/> は Windows 専用のため、本クラスも Windows でのみ動作します
/// </remarks>
public static class ScreenVisibleArea
{
    /// <summary>DPI スケールの未取得（0 以下）を 100% とみなすときの値。</summary>
    private const double DefaultScale = 1.0;

    /// <summary>仮想画面の外接矩形を DIP（論理ピクセル）で取得します。</summary>
    /// <param name="scale">DIP から物理ピクセルへの換算に使用する DPI スケール。0 以下は 100% とみなします。</param>
    /// <returns>仮想画面の外接矩形（DIP、負座標を含みます）。</returns>
    public static RectD GetVisibleArea(double scale)
    {
        var bounds = Win32Display.GetVirtualScreenBounds();
        var dpiScale = Normalize(scale);

        return new RectD(
            bounds.X / dpiScale,
            bounds.Y / dpiScale,
            bounds.Width / dpiScale,
            bounds.Height / dpiScale);
    }

    /// <summary>保存されたウィンドウ位置を表示領域内へ補正します。</summary>
    /// <param name="saved">保存されたウィンドウ左上の位置（画面座標・DIP）。</param>
    /// <param name="windowSize">ウィンドウのサイズ（DIP）。</param>
    /// <param name="scale">DIP から物理ピクセルへの換算に使用する DPI スケール。0 以下は 100% とみなします。</param>
    /// <returns>補正後のウィンドウ左上の位置（DIP）。</returns>
    public static PointD Clamp(PointD saved, SizeD windowSize, double scale)
        => WindowPlacement.ClampToVisibleArea(saved, windowSize, GetVisibleArea(scale));

    /// <summary>DIP の値を物理ピクセルへ換算します。</summary>
    /// <param name="dip">DIP（論理ピクセル）の値。</param>
    /// <param name="scale">DIP から物理ピクセルへの換算に使用する DPI スケール。0 以下は 100% とみなします。</param>
    /// <returns>物理ピクセル値（四捨五入）。</returns>
    /// <remarks>丸め規則（四捨五入）を一元管理し、ウィンドウの位置とサイズで規則がずれないようにします。</remarks>
    public static int ToPhysical(double dip, double scale)
        => (int)Math.Round(dip * Normalize(scale));

    /// <summary>物理ピクセルの値を DIP へ換算します。</summary>
    /// <param name="physical">物理ピクセルの値。</param>
    /// <param name="scale">DIP から物理ピクセルへの換算に使用する DPI スケール。0 以下は 100% とみなします。</param>
    /// <returns>DIP（論理ピクセル）の値。</returns>
    public static double ToDip(int physical, double scale) => physical / Normalize(scale);

    /// <summary>DPI スケールを正規化します。</summary>
    /// <param name="scale">DPI スケール。</param>
    /// <returns>有効なスケール。0 以下だった場合は 100%。</returns>
    /// <remarks>描画前や終了処理中は RenderScaling 等が 0 を返すことがあるため、その場合に備えます。</remarks>
    private static double Normalize(double scale) => scale > 0 ? scale : DefaultScale;
}
