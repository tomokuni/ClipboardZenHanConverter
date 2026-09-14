using System;

namespace EsUtil.ClipboardZenHanConverter.Core.Helpers;

/// <summary>上下 2 分割レイアウトの分割比率を扱う純粋ロジックを提供する静的クラス。</summary>
/// <remarks>提供機能: <br/>
/// - ドラッグ量から新しい分割比率を求める計算<br/>
/// - 比率の上下限によるクランプ<br/><br/>
/// 特徴: <br/>
/// - UI フレームワークに依存しない純粋関数として提供し、ヘッドレスで単体テスト可能<br/>
/// - 比率（0.0〜1.0）で表すため、UI 側は「上ペイン = 比率」「下ペイン = 1 - 比率」の
///   スターサイズ（相対サイズ）へ割り当てるだけでよい<br/>
/// - 比率で保持するため、ウィンドウの高さが変化しても上下の比率が自動的に維持される<br/><br/>
/// 注意点: <br/>
/// - 計算に使用する高さ（<c>trackHeight</c>）は上下 2 ペインの合計とし、分割バーの太さは含めません
/// </remarks>
public static class SplitLayout
{
    /// <summary>既定の分割比率。</summary>
    /// <value>上下を等分する 0.5。</value>
    public const double DefaultRatio = 0.5;

    /// <summary>分割比率の下限。</summary>
    /// <value>上ペインを操作不能なほど小さくしないための 0.1。</value>
    public const double MinRatio = 0.1;

    /// <summary>分割比率の上限。</summary>
    /// <value>下ペインを操作不能なほど小さくしないための 0.9。</value>
    public const double MaxRatio = 0.9;

    /// <summary>分割比率を有効範囲へ補正します。</summary>
    /// <param name="ratio">補正する分割比率。NaN の場合は <see cref="DefaultRatio"/> を返します。</param>
    /// <returns>上下限の範囲内に収めた分割比率。</returns>
    public static double ClampRatio(double ratio)
        => double.IsNaN(ratio) ? DefaultRatio : Math.Clamp(ratio, MinRatio, MaxRatio);

    /// <summary>ドラッグ量から新しい分割比率を求めます。</summary>
    /// <param name="startRatio">ドラッグ開始時の分割比率。</param>
    /// <param name="deltaY">ドラッグ開始点からの移動量（下方が正）。</param>
    /// <param name="trackHeight">上下 2 ペインの合計高さ。分割バーの太さは含めません。</param>
    /// <returns>補正後の分割比率。</returns>
    /// <remarks>処理フロー: <br/>
    /// 1. 高さが 0 以下の場合は割合を求めてられないため、開始時の比率をそのまま返す<br/>
    /// 2. 開始時の上ペイン高さ（開始比率 × 合計高さ）に移動量を加える<br/>
    /// 3. 合計高さで割って新しい比率とし、上下限で補正する</remarks>
    public static double RatioFromDrag(double startRatio, double deltaY, double trackHeight)
    {
        if (trackHeight <= 0 || !double.IsFinite(deltaY))
            return ClampRatio(startRatio);

        return ClampRatio(((startRatio * trackHeight) + deltaY) / trackHeight);
    }

    /// <summary>分割比率から上ペインの割合を求めます。</summary>
    /// <param name="ratio">分割比率。</param>
    /// <returns>上ペインの割合（0.0〜1.0）。</returns>
    public static double TopWeight(double ratio) => ClampRatio(ratio);

    /// <summary>分割比率から下ペインの割合を求めます。</summary>
    /// <param name="ratio">分割比率。</param>
    /// <returns>下ペインの割合（0.0〜1.0）。</returns>
    public static double BottomWeight(double ratio) => 1.0 - ClampRatio(ratio);
}
