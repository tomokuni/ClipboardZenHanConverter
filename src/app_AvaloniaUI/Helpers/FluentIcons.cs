using Avalonia.Media;
using ClipboardZenHanConverter.Core.Icons;

namespace ClipboardZenHanConverter.App.AvaloniaUI.Helpers;

/// <summary>アプリで使用する Fluent Icons のアイコン形状を提供する静的クラス。</summary>
/// <remarks>提供機能: <br/>
/// - Core の FluentIconData（SVG のパスデータ）から生成した Avalonia のアイコン形状の提供<br/><br/>
/// 特徴: <br/>
/// - パスデータは Core が単一所有し、Avalonia の形状への変換のみを担う<br/>
/// - MewUI 版・WinUI 3 版と同じ SVG を共有するため、3 つの UI でアイコン形状が一致する<br/>
/// - 生成は初回アクセスの一度だけ行い、以降は生成済みの形状を返却</remarks>
public static class FluentIcons
{
    /// <summary>Convert Range アイコンの形状（初回アクセス時に生成）。</summary>
    private static Geometry? s_convertRange;

    /// <summary>Settings アイコンの形状（初回アクセス時に生成）。</summary>
    private static Geometry? s_settings;

    /// <summary>Fluent Icons の「Convert Range」アイコン形状を取得します。</summary>
    /// <value>24px グリッドで描かれた Convert Range のベクター形状。</value>
    public static Geometry ConvertRange => s_convertRange ??= ParseGeometry(FluentIconData.ConvertRange);

    /// <summary>Fluent Icons の「Settings」アイコン形状を取得します。</summary>
    /// <value>24px グリッドで描かれた Settings のベクター形状。</value>
    public static Geometry Settings => s_settings ??= ParseGeometry(FluentIconData.Settings);

    /// <summary>SVG のパスデータを Avalonia の形状へ変換します。</summary>
    /// <param name="pathData">SVG のパスデータ（d 属性の値）。</param>
    /// <returns>変換したベクター形状。</returns>
    private static Geometry ParseGeometry(string pathData) => Geometry.Parse(pathData);
}
