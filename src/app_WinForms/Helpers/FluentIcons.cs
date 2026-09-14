using ClipboardZenHanConverter.App.WinForms.ViewModels;
using ClipboardZenHanConverter.Core.Icons;
using System.Drawing.Drawing2D;

namespace ClipboardZenHanConverter.App.WinForms.Helpers;

/// <summary>アプリで使用する Fluent Icons のアイコン画像を提供する静的クラス。</summary>
/// <remarks>提供機能: <br/>
/// - Core の FluentIconData（SVG のパスデータ）から生成したアイコン画像の提供<br/><br/>
/// 特徴: <br/>
/// - パスデータは Core が単一所有し、GDI+ の画像への変換のみを担う<br/>
/// - MewUI 版・WinUI 3 版・Avalonia UI 版と同じ SVG を共有するため、4 つの UI でアイコン形状が一致する<br/>
/// - 生成は初回アクセスの一度だけ行う（アイコンは 2 種・サイズ固定のため、キャッシュは自然に上限 2 件）<br/>
/// - 解析結果（GraphicsPath）は画像の生成後に破棄し、保持しない</remarks>
public static class FluentIcons
{
    /// <summary>ナビゲーションペインのアイコンサイズ（論理ピクセル）。</summary>
    private const int IconSize = 16;

    /// <summary>SVG の viewBox の一辺（24×24）。</summary>
    private const float ViewBoxSize = 24f;

    /// <summary>アイコンの描画色（Fluent の TextFillColorPrimary 相当）。</summary>
    private static readonly Color IconColor = Color.FromArgb(0x1B, 0x1B, 0x1B);

    /// <summary>Convert Range アイコンの画像（初回アクセス時に生成）。</summary>
    private static Image? s_convertRange;

    /// <summary>Settings アイコンの画像（初回アクセス時に生成）。</summary>
    private static Image? s_settings;

    /// <summary>Convert Range アイコン（ホーム）の画像を取得します。</summary>
    /// <value>16×16 で描画した Convert Range アイコン。</value>
    public static Image ConvertRange => s_convertRange ??= Render(FluentIconData.ConvertRange);

    /// <summary>Settings アイコン（設定）の画像を取得します。</summary>
    /// <value>16×16 で描画した Settings アイコン。</value>
    public static Image Settings => s_settings ??= Render(FluentIconData.Settings);

    /// <summary>ナビゲーション項目の種別に対応するアイコン画像を取得します。</summary>
    /// <param name="icon">アイコンの種別。</param>
    /// <returns>種別に対応するアイコン画像。</returns>
    /// <remarks>画像は共有インスタンスのため、呼び出し側で破棄しないでください。</remarks>
    public static Image Get(NavigationIcon icon) => icon switch
    {
        NavigationIcon.ConvertRange => ConvertRange,
        NavigationIcon.Settings => Settings,
        _ => throw new ArgumentOutOfRangeException(nameof(icon), icon, "未対応のアイコン種別です。"),
    };

    /// <summary>SVG のパスデータをアイコン画像へ描画します。</summary>
    /// <param name="pathData">SVG のパスデータ（d 属性の値）。</param>
    /// <returns>描画したアイコン画像。</returns>
    /// <remarks>Fluent Icons の regular は線そのものを輪郭として表した塗りつぶし図形のため、パスを塗りつぶします。<br/>
    /// 抜き（歯車のリング・中央の輪）は部分パスの重なり回数で表現されているため、EvenOdd 規則で描画します。</remarks>
    private static Image Render(string pathData)
    {
        var bitmap = new Bitmap(IconSize, IconSize);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);

        using var path = SvgPathParser.Parse(pathData);
        using var transform = new Matrix();
        transform.Scale(IconSize / ViewBoxSize, IconSize / ViewBoxSize);
        path.Transform(transform);

        using var brush = new SolidBrush(IconColor);
        graphics.FillPath(brush, path);

        return bitmap;
    }
}
