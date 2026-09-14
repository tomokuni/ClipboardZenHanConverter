namespace ClipboardZenHanConverter.Core.Native;

/// <summary>物理ピクセル座標系の矩形を表します。</summary>
/// <remarks>仮想画面（全モニタを包含する外接矩形）など、UI フレームワークに依存しない座標を扱います。<br/>
/// マルチモニタ構成では原点より左上のモニタが存在するため、負座標を含みます。</remarks>
/// <param name="X">左上の X 座標（物理ピクセル）。</param>
/// <param name="Y">左上の Y 座標（物理ピクセル）。</param>
/// <param name="Width">幅（物理ピクセル）。</param>
/// <param name="Height">高さ（物理ピクセル）。</param>
public readonly record struct PixelBounds(int X, int Y, int Width, int Height);
