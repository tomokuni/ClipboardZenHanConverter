namespace EsUtil.ClipboardZenHanConverter.Core.Geometry;

/// <summary>DIP（デバイス非依存ピクセル）座標のサイズを表します。</summary>
/// <param name="Width">幅（DIP）。</param>
/// <param name="Height">高さ（DIP）。</param>
/// <remarks>物理ピクセルの寸法は <see cref="Native.PixelBounds"/> で扱います。</remarks>
public readonly record struct SizeD(double Width, double Height);
