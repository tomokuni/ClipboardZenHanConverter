using System.Runtime.InteropServices;

namespace ClipboardZenHanConverter.Core.Native;

/// <summary>仮想画面（全モニタを包含する外接矩形）を取得する Win32 API ラッパー。</summary>
/// <remarks>NativeAOT 対応のため <c>LibraryImport</c> によるソース生成 P/Invoke を使用します。<br/>
/// UI フレームワークに依存しないため Core 層に配置します。<br/>
/// 返却値は物理ピクセルであり、DIP への換算は呼び出し側の DPI スケールで行います。</remarks>
public static partial class Win32Display
{
    private const int SmXVirtualScreen = 76;
    private const int SmYVirtualScreen = 77;
    private const int SmCxVirtualScreen = 78;
    private const int SmCyVirtualScreen = 79;

    /// <summary>システムメトリック値を取得します。</summary>
    /// <param name="nIndex">取得するメトリックのインデックス。</param>
    /// <returns>メトリック値（物理ピクセル）。</returns>
    [LibraryImport("user32.dll")]
    private static partial int GetSystemMetrics(int nIndex);

    /// <summary>仮想画面（全モニタの外接矩形）を物理ピクセルで取得します。</summary>
    /// <returns>仮想画面の外接矩形（負座標を含みます）。</returns>
    public static PixelBounds GetVirtualScreenBounds() => new(
        GetSystemMetrics(SmXVirtualScreen),
        GetSystemMetrics(SmYVirtualScreen),
        GetSystemMetrics(SmCxVirtualScreen),
        GetSystemMetrics(SmCyVirtualScreen));
}
