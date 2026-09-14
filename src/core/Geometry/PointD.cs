namespace ClipboardZenHanConverter.Core.Geometry;

/// <summary>DIP（デバイス非依存ピクセル）座標の点を表します。</summary>
/// <param name="X">X 座標（DIP）。</param>
/// <param name="Y">Y 座標（DIP）。</param>
/// <remarks>物理ピクセルの座標は <see cref="Native.PixelBounds"/> で扱います。<br/>
/// マルチモニタ構成では原点より左上のモニタが存在するため、負座標を含みます。</remarks>
public readonly record struct PointD(double X, double Y);
