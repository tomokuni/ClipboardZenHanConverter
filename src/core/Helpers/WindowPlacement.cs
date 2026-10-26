using EsUtil.Platform.Core.Geometry;
using System;

namespace EsUtil.ZenHanConverter.Core.Helpers;

/// <summary>保存されたウィンドウ位置を表示領域内へ補正する純粋ロジックを提供する静的クラス。</summary>
/// <remarks>提供機能: <br/>
/// - 一部が表示領域外のウィンドウを、はみ出し分を詰めて表示領域内へ収める補正<br/>
/// - 完全に表示領域外のウィンドウを原点へ戻す補正<br/><br/>
/// 特徴: <br/>
/// - UI フレームワークに依存しない純粋関数として提供し、ヘッドレスで単体テスト可能<br/>
/// - 座標は DIP（<see cref="EsPoint"/> / <see cref="EsSize"/> / <see cref="EsRect"/>）で扱う<br/>
/// - モニタ構成の違いは呼び出し側が表示領域（全モニタの外接矩形）として与える</remarks>
public static class WindowPlacement
{
    /// <summary>ウィンドウを移動させる原点（プライマリモニタの左上）。</summary>
    private static readonly EsPoint s_origin = new(0, 0);

    /// <summary>保存された位置を表示領域内へ補正します。</summary>
    /// <param name="saved">保存されたウィンドウ左上の位置（画面座標・DIP）。</param>
    /// <param name="windowSize">ウィンドウのサイズ（DIP）。</param>
    /// <param name="visibleArea">表示領域の外接矩形（DIP、負座標を含みます）。</param>
    /// <returns>補正後のウィンドウ左上の位置。</returns>
    /// <remarks>処理フロー: <br/>
    /// 1. ウィンドウが表示領域と少しでも重なるかを判定<br/>
    /// 2. 完全に領域外の場合は原点を返却<br/>
    /// 3. 重なる場合は、はみ出した辺を表示領域の内側へ詰めて返却<br/><br/>
    /// 注意点: <br/>
    /// - ウィンドウが表示領域より大きい場合は、その軸の表示領域先頭へ寄せます</remarks>
    public static EsPoint ClampToVisibleArea(EsPoint saved, EsSize windowSize, EsRect visibleArea)
    {
        if (!Intersects(saved, windowSize, visibleArea))
            return s_origin;

        return new EsPoint(
            ClampAxis(saved.X, visibleArea.X, visibleArea.Right, windowSize.Width),
            ClampAxis(saved.Y, visibleArea.Y, visibleArea.Bottom, windowSize.Height));
    }

    /// <summary>ウィンドウが表示領域と重なるかを判定します。</summary>
    /// <param name="position">ウィンドウ左上の位置。</param>
    /// <param name="size">ウィンドウのサイズ。</param>
    /// <param name="area">表示領域の外接矩形。</param>
    /// <returns>僅かでも重なる場合は true。</returns>
    private static bool Intersects(EsPoint position, EsSize size, EsRect area)
        => position.X < area.Right
        && position.X + size.Width > area.X
        && position.Y < area.Bottom
        && position.Y + size.Height > area.Y;

    /// <summary>指定された軸の座標を表示領域内へ収まるように補正します。</summary>
    /// <param name="value">補正する座標。</param>
    /// <param name="min">表示領域の先頭座標。</param>
    /// <param name="max">表示領域の終端座標。</param>
    /// <param name="size">その軸のウィンドウサイズ。</param>
    /// <returns>補正後の座標。</returns>
    private static double ClampAxis(double value, double min, double max, double size)
    {
        // ウィンドウが表示領域より大きい場合は終端が先頭を下回るため、先頭へ寄せる
        var last = Math.Max(min, max - size);
        return Math.Clamp(value, min, last);
    }
}
