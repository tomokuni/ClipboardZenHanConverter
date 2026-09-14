using Aprillz.MewUI.Rendering;
using EsUtil.ClipboardZenHanConverter.Core.Icons;

namespace EsUtil.ClipboardZenHanConverter.App.MewUI.Helpers;

/// <summary>アプリで使用する Fluent Icons のアイコン形状を提供する静的クラス。</summary>
/// <remarks>提供機能: <br/>
/// - Core の FluentIconData（SVG のパスデータ）から生成した MewUI のアイコン形状の提供<br/><br/>
/// 特徴: <br/>
/// - パスデータは Core が単一所有し、MewUI の形状への変換のみを担う<br/>
/// - 生成は初回アクセスの一度だけ行い、以降は生成済みの形状を返却</remarks>
public static class FluentIcons
{
    /// <summary>Convert Range アイコンの形状（初回アクセス時に生成）。</summary>
    private static PathGeometry? s_convertRange;

    /// <summary>Settings アイコンの形状（初回アクセス時に生成）。</summary>
    private static PathGeometry? s_settings;

    /// <summary>Fluent Icons の「Convert Range」アイコン形状を取得します。</summary>
    /// <value>24px グリッドで描かれた Convert Range のベクター形状。</value>
    public static PathGeometry ConvertRange => s_convertRange ??= PathGeometry.Parse(FluentIconData.ConvertRange);

    /// <summary>Fluent Icons の「Settings」アイコン形状を取得します。</summary>
    /// <value>24px グリッドで描かれた Settings のベクター形状。</value>
    public static PathGeometry Settings => s_settings ??= PathGeometry.Parse(FluentIconData.Settings);
}
