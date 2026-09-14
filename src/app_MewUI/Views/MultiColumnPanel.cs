using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using EsUtil.Algorithm;

namespace ClipboardZenHanConverter.App.MewUI.Views;

/// <summary>子要素を縦方向の複数列へ配置するパネル。</summary>
/// <remarks>提供機能: <br/>
/// - 子要素を列ごとに縦へ積み、最大列高さを最小化するマルチカラム配置<br/>
/// - 行間（RowSpace）・列間（ColumnSpace）・列数上限（ColumnLimit）の指定<br/><br/>
/// 特徴: <br/>
/// - 配置計算は UI 非依存の MultiColumnLayoutEngine へ委譲し、実測した子要素サイズを入力とする<br/>
/// - 算出された各アイテムの座標とサイズをそのまま配置へ反映<br/>
/// - 測定結果を保持し、配置時の再計算を排除</remarks>
public sealed class MultiColumnPanel : Panel
{
    /// <summary>行間スペースの既定値（DIP）。</summary>
    public const double DefaultRowSpace = 4;

    /// <summary>列間スペースの既定値（DIP）。</summary>
    public const double DefaultColumnSpace = 32;

    /// <summary>列数上限の既定値。</summary>
    public const int DefaultColumnLimit = 1;

    /// <summary>行間スペース（DIP）のプロパティ。</summary>
    public static readonly MewProperty<double> RowSpaceProperty =
        MewProperty<double>.Register<MultiColumnPanel>(nameof(RowSpace), DefaultRowSpace, MewPropertyOptions.AffectsLayout);

    /// <summary>列間スペース（DIP）のプロパティ。</summary>
    public static readonly MewProperty<double> ColumnSpaceProperty =
        MewProperty<double>.Register<MultiColumnPanel>(nameof(ColumnSpace), DefaultColumnSpace, MewPropertyOptions.AffectsLayout);

    /// <summary>列数上限のプロパティ。</summary>
    public static readonly MewProperty<int> ColumnLimitProperty =
        MewProperty<int>.Register<MultiColumnPanel>(nameof(ColumnLimit), DefaultColumnLimit, MewPropertyOptions.AffectsLayout);

    /// <summary>直近の測定で算出した各アイテムの配置矩形（X, Y, Width, Height）。</summary>
    private IReadOnlyList<(double X, double Y, double Width, double Height)>? _itemLayouts;

    /// <summary>行間スペース（DIP）を取得または設定します。</summary>
    /// <value>列内で隣り合うアイテムの垂直方向の間隔（0 以上）。</value>
    public double RowSpace
    {
        get => GetValue(RowSpaceProperty);
        set => SetValue(RowSpaceProperty, value);
    }

    /// <summary>列間スペース（DIP）を取得または設定します。</summary>
    /// <value>隣り合う列の水平方向の間隔（0 以上）。</value>
    public double ColumnSpace
    {
        get => GetValue(ColumnSpaceProperty);
        set => SetValue(ColumnSpaceProperty, value);
    }

    /// <summary>使用する列数の上限を取得または設定します。</summary>
    /// <value>1 以上の列数。実際の列数は幅に応じて減ることがあります。</value>
    public int ColumnLimit
    {
        get => GetValue(ColumnLimitProperty);
        set => SetValue(ColumnLimitProperty, value);
    }

    /// <summary>子要素を測定し、最適なマルチカラム配置を算出します。</summary>
    /// <param name="availableSize">割り当て可能なサイズ。</param>
    /// <returns>使用幅と最大列高さから求めた必要サイズ。</returns>
    /// <remarks>処理フロー: <br/>
    /// 1. 表示中の子要素を幅制約の下で測定<br/>
    /// 2. 測定結果をアイテムサイズとしてマルチカラム配置を算出<br/>
    /// 3. 使用幅と最大列高さをパネルの必要サイズとして返却</remarks>
    protected override Size MeasureContent(Size availableSize)
    {
        var contentSize = availableSize.Deflate(Padding);
        var itemSizes = MeasureChildren(contentSize.Width);

        if (itemSizes.Length == 0)
        {
            _itemLayouts = null;

            // 子要素が無い場合はパディング分のみを必要サイズとする
            return new Size(Padding.HorizontalThickness, Padding.VerticalThickness);
        }

        // 幅が確定しない場合は最大アイテム幅を上限とし、単列相当の配置とする
        var widthLimit = contentSize.Width;
        if (!double.IsFinite(widthLimit) || widthLimit <= 0)
            widthLimit = GetMaxWidth(itemSizes);

        var engine = new MultiColumnLayoutEngine(
            itemSizes,
            (Math.Max(0, RowSpace), Math.Max(0, ColumnSpace)),
            Math.Max(1, ColumnLimit));

        var (usedWidth, minHeight) = engine.Solve(widthLimit);
        _itemLayouts = engine.GetLastItemLayouts();

        return new Size(usedWidth, minHeight).Inflate(Padding);
    }

    /// <summary>算出済みの配置矩形に従って子要素を配置します。</summary>
    /// <param name="bounds">割り当てられた矩形。</param>
    /// <remarks>配置計算は測定時に完了しているため、ここでは結果の反映のみを行います。</remarks>
    protected override void ArrangeContent(Rect bounds)
    {
        var layouts = _itemLayouts;
        if (layouts is null)
            return;

        var contentBounds = bounds.Deflate(Padding);
        var index = 0;

        foreach (var child in ChildrenList)
        {
            if (child is not UIElement ui || !ui.IsVisible)
                continue;

            if (index >= layouts.Count)
                break;

            var (x, y, width, height) = layouts[index];
            ui.Arrange(new Rect(contentBounds.X + x, contentBounds.Y + y, width, height));
            index++;
        }
    }

    /// <summary>表示中の子要素を測定し、アイテムサイズ配列を生成します。</summary>
    /// <param name="availableWidth">子要素へ与える幅制約。無限大も許容します。</param>
    /// <returns>表示中の子要素の測定サイズ（幅, 高さ）配列。</returns>
    private (double Width, double Height)[] MeasureChildren(double availableWidth)
    {
        var visibleCount = 0;

        foreach (var child in ChildrenList)
        {
            if (child is not UIElement ui || !ui.IsVisible)
                continue;

            ui.Measure(new Size(availableWidth, double.PositiveInfinity));
            visibleCount++;
        }

        var sizes = new (double Width, double Height)[visibleCount];
        var index = 0;

        foreach (var child in ChildrenList)
        {
            if (child is not UIElement ui || !ui.IsVisible)
                continue;

            var desired = ui.DesiredSize;
            sizes[index] = (desired.Width, desired.Height);
            index++;
        }

        return sizes;
    }

    /// <summary>アイテムサイズ配列から最大幅を取得します。</summary>
    /// <param name="itemSizes">アイテムサイズ配列。</param>
    /// <returns>アイテム幅の最大値。</returns>
    private static double GetMaxWidth((double Width, double Height)[] itemSizes)
    {
        var maxWidth = 0.0;

        foreach (var (width, _) in itemSizes)
        {
            if (width > maxWidth)
                maxWidth = width;
        }

        return maxWidth;
    }
}
