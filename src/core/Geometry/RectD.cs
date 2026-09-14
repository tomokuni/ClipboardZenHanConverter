namespace EsUtil.ClipboardZenHanConverter.Core.Geometry;

/// <summary>DIP（デバイス非依存ピクセル）座標の矩形を表します。</summary>
/// <param name="X">左上の X 座標（DIP）。</param>
/// <param name="Y">左上の Y 座標（DIP）。</param>
/// <param name="Width">幅（DIP）。</param>
/// <param name="Height">高さ（DIP）。</param>
/// <remarks>物理ピクセルの矩形は <see cref="Native.PixelBounds"/> で扱います。<br/>
/// マルチモニタ構成では原点より左上のモニタが存在するため、負座標を含みます。</remarks>
public readonly record struct RectD(double X, double Y, double Width, double Height)
{
    /// <summary>左端の X 座標（DIP）を取得します。</summary>
    /// <value>矩形の左端の X 座標。</value>
    public double Left => X;

    /// <summary>右端の X 座標（DIP）を取得します。</summary>
    /// <value>矩形の右端の X 座標（X + Width）。</value>
    public double Right => X + Width;

    /// <summary>上端の Y 座標（DIP）を取得します。</summary>
    /// <value>矩形の上端の Y 座標。</value>
    public double Top => Y;

    /// <summary>下端の Y 座標（DIP）を取得します。</summary>
    /// <value>矩形の下端の Y 座標（Y + Height）。</value>
    public double Bottom => Y + Height;
}
