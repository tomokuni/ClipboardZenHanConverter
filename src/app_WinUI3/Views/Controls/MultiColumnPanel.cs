// このソースコードは、UTF-8、LF で作成します。

using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;

using EsUtil.Algorithm;
using static EsUtil.Algorithm.MultiColumnLayoutEngine;

namespace EsUtil.ClipboardZenHanConverter.App.WinUI.Views.Controls;

/// <summary>子要素を縦方向の複数列へ配置し、配置に必要なサイズを算出するパネル。</summary>
/// <remarks>提供機能: <br/>
/// - ItemsSource（ItemTemplate 併用）と XAML で直接追加した子要素の同時配置<br/>
/// - 行間（RowSpace）・列間（ColumnSpace）・最大列数（ColumnLimit）の指定<br/>
/// - 配置アルゴリズム（Method: DynamicProgramming / BinarySearch / Greedy）の選択<br/>
/// - 直近の配置結果（LayoutUsedWidth・LayoutMinHeight・LayoutColumnCount・LayoutColumnSegments）の公開<br/><br/>
/// 特徴: <br/>
/// - 配置計算は UI 非依存の MultiColumnLayoutEngine へ委譲し、実測した子要素サイズを入力とする<br/>
/// - 算出された各アイテムの座標とサイズをそのまま配置へ反映<br/><br/>
/// 注意: <br/>
/// - 配置に影響するプロパティを変更すると、保持している配置結果を破棄して再測定する<br/>
/// </remarks>
public sealed class MultiColumnPanel : DualSourceItemsPanel
{
    /// <summary>直近の測定で算出した配置結果。</summary>
    private MultiColumnLayoutEngine? _layout;

    /// <summary>行間スペースを指定する DependencyProperty。</summary>
    public static readonly DependencyProperty RowSpaceProperty = DependencyProperty.Register(
        nameof(RowSpace),
        typeof(double),
        typeof(MultiColumnPanel),
        new PropertyMetadata(8.0, OnLayoutPropertyChanged));

    /// <summary>列間スペースを指定する DependencyProperty。</summary>
    public static readonly DependencyProperty ColumnSpaceProperty = DependencyProperty.Register(
        nameof(ColumnSpace),
        typeof(double),
        typeof(MultiColumnPanel),
        new PropertyMetadata(8.0, OnLayoutPropertyChanged));

    /// <summary>最大列数を指定する DependencyProperty。</summary>
    public static readonly DependencyProperty ColumnLimitProperty = DependencyProperty.Register(
        nameof(ColumnLimit),
        typeof(int),
        typeof(MultiColumnPanel),
        new PropertyMetadata(10, OnLayoutPropertyChanged));

    /// <summary>使用する配置アルゴリズムを指定する DependencyProperty。</summary>
    public static readonly DependencyProperty MethodProperty = DependencyProperty.Register(
        nameof(Method),
        typeof(Method),
        typeof(MultiColumnPanel),
        new PropertyMetadata(Method.BinarySearch, OnLayoutPropertyChanged));

    /// <summary>直近の配置で使用した幅を公開する DependencyProperty。</summary>
    public static readonly DependencyProperty LayoutUsedWidthProperty = DependencyProperty.Register(
        nameof(LayoutUsedWidth),
        typeof(double?),
        typeof(MultiColumnPanel),
        new PropertyMetadata(null));

    /// <summary>直近の配置で算出した最大列高さを公開する DependencyProperty。</summary>
    public static readonly DependencyProperty LayoutMinHeightProperty = DependencyProperty.Register(
        nameof(LayoutMinHeight),
        typeof(double?),
        typeof(MultiColumnPanel),
        new PropertyMetadata(null));

    /// <summary>直近の配置で算出した列数を公開する DependencyProperty。</summary>
    public static readonly DependencyProperty LayoutColumnCountProperty = DependencyProperty.Register(
        nameof(LayoutColumnCount),
        typeof(int?),
        typeof(MultiColumnPanel),
        new PropertyMetadata(null));

    /// <summary>直近の配置で算出した列セグメント情報を公開する DependencyProperty。</summary>
    public static readonly DependencyProperty LayoutColumnSegmentsProperty = DependencyProperty.Register(
        nameof(LayoutColumnSegments),
        typeof(string),
        typeof(MultiColumnPanel),
        new PropertyMetadata(null));

    /// <summary>行間スペース（ピクセル）を取得または設定する。</summary>
    /// <value>列内で隣り合うアイテムの垂直方向の間隔（0 以上、既定値は 8.0）。</value>
    public double RowSpace
    {
        get => (double)GetValue(RowSpaceProperty);
        set => SetValue(RowSpaceProperty, value);
    }

    /// <summary>列間スペース（ピクセル）を取得または設定する。</summary>
    /// <value>隣り合う列の水平方向の間隔（0 以上、既定値は 8.0）。</value>
    public double ColumnSpace
    {
        get => (double)GetValue(ColumnSpaceProperty);
        set => SetValue(ColumnSpaceProperty, value);
    }

    /// <summary>最大列数を取得または設定する。</summary>
    /// <value>1 以上の列数（実際の列数は幅に応じて減ることがある。既定値は 10）。</value>
    public int ColumnLimit
    {
        get => (int)GetValue(ColumnLimitProperty);
        set => SetValue(ColumnLimitProperty, value);
    }

    /// <summary>使用する配置アルゴリズムを取得または設定する。</summary>
    /// <value>DynamicProgramming / BinarySearch / Greedy（既定値は BinarySearch）。</value>
    public Method Method
    {
        get => (Method)GetValue(MethodProperty);
        set => SetValue(MethodProperty, value);
    }

    /// <summary>直近の配置で使用した幅（ピクセル）を取得する。</summary>
    /// <value>未計算の場合は null。</value>
    public double? LayoutUsedWidth
    {
        get => (double?)GetValue(LayoutUsedWidthProperty);
        private set => SetValue(LayoutUsedWidthProperty, value);
    }

    /// <summary>直近の配置で算出した最大列高さ（ピクセル）を取得する。</summary>
    /// <value>未計算の場合は null。</value>
    public double? LayoutMinHeight
    {
        get => (double?)GetValue(LayoutMinHeightProperty);
        private set => SetValue(LayoutMinHeightProperty, value);
    }

    /// <summary>直近の配置で算出した列数を取得する。</summary>
    /// <value>未計算の場合は null。</value>
    public int? LayoutColumnCount
    {
        get => (int?)GetValue(LayoutColumnCountProperty);
        private set => SetValue(LayoutColumnCountProperty, value);
    }

    /// <summary>直近の配置で算出した列セグメント情報を取得する。</summary>
    /// <value>各列の終端アイテム番号を `[0,3,6]` 形式で表した文字列（未計算の場合は null）。</value>
    public string? LayoutColumnSegments
    {
        get => (string?)GetValue(LayoutColumnSegmentsProperty);
        private set => SetValue(LayoutColumnSegmentsProperty, value);
    }

    /// <summary>配置に影響する DependencyProperty が変更されたときの処理を行う。</summary>
    /// <param name="d">変更されたパネル（null 不可）。</param>
    /// <param name="e">変更されたプロパティと新旧の値（null 不可）。</param>
    /// <remarks>保持している配置結果を破棄し、配置結果プロパティの公開値をリセットして再測定を要求する。</remarks>
    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var panel = (MultiColumnPanel)d;

        panel._layout = null;
        panel.UpdateLayoutResultProperties();
        panel.InvalidateMeasure();
    }

    /// <summary>保持している配置結果を配置結果プロパティへ反映する。</summary>
    /// <remarks>配置結果が未計算の場合はすべて null へ戻す。</remarks>
    private void UpdateLayoutResultProperties()
    {
        if (_layout is null)
        {
            LayoutUsedWidth = null;
            LayoutMinHeight = null;
            LayoutColumnCount = null;
            LayoutColumnSegments = null;

            return;
        }

        var result = _layout.GetLastResult();
        LayoutUsedWidth = result.UsedWidth;
        LayoutMinHeight = result.MinHeight;

        var segments = _layout.GetLastColumnSegments();
        LayoutColumnCount = segments.Length;
        LayoutColumnSegments = "[" + string.Join(",", segments.Select(seg => seg.EndIdx)) + "]";
    }

    /// <summary>ItemsSource の変更後に配置結果を破棄する。</summary>
    protected override void OnItemsSourceChangedInternal()
    {
        base.OnItemsSourceChangedInternal();

        _layout = null;
        UpdateLayoutResultProperties();
    }

    /// <summary>ItemTemplate の変更後に配置結果を破棄する。</summary>
    protected override void OnItemTemplateChangedInternal()
    {
        base.OnItemTemplateChangedInternal();

        _layout = null;
        UpdateLayoutResultProperties();
    }

    /// <summary>ItemsSourceFirst の変更後に配置結果を破棄する。</summary>
    protected override void OnItemsSourceFirstChangedInternal()
    {
        base.OnItemsSourceFirstChangedInternal();

        _layout = null;
        UpdateLayoutResultProperties();
    }

    /// <summary>子要素を測定し、最適なマルチカラム配置を算出する。</summary>
    /// <param name="availableSize">割り当て可能なサイズ。</param>
    /// <returns>使用幅と最大列高さから求めた必要サイズ。</returns>
    /// <remarks>処理フロー: <br/>
    /// 1. 子要素が無い場合は (0, 0) を返す<br/>
    /// 2. 各子要素を無限サイズで測定し、自然サイズを取得する<br/>
    /// 3. 測定結果をアイテムサイズとして最適な配置を計算する<br/>
    /// 4. 算出した使用幅と最大列高さを必要サイズとして返す<br/>
    /// </remarks>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (Children.Count == 0)
        {
            return new Size(0, 0);
        }

        var childElements = ChildElements;
        if (childElements.Count == 0)
        {
            return new Size(0, 0);
        }

        var inf = new Size(double.PositiveInfinity, double.PositiveInfinity);
        foreach (var el in childElements)
        {
            el.Measure(inf);
        }

        var items = childElements.Select(e => (e.DesiredSize.Width, e.DesiredSize.Height)).ToArray();

        _layout ??= new MultiColumnLayoutEngine(items, (RowSpace, ColumnSpace), ColumnLimit);
        _layout.CurrentMethod = Method;

        // 幅が確定しない場合はアイテムの自然サイズを基準に配置を解く
        double widthLimit = double.IsInfinity(availableSize.Width)
            ? double.PositiveInfinity
            : availableSize.Width;

        var (usedWidth, minHeight) = _layout.Solve(widthLimit);
        Debug.WriteLine($"(UsedWidth, MinHeight): ({usedWidth}, {minHeight})");

        UpdateLayoutResultProperties();

        return new Size(usedWidth, minHeight);
    }

    /// <summary>測定時に算出した配置結果に従って子要素を配置する。</summary>
    /// <param name="finalSize">割り当てられたサイズ。</param>
    /// <returns>実際に使用したサイズ。</returns>
    /// <remarks>処理フロー: <br/>
    /// 1. 子要素が無い、または配置結果が無い場合は何もしない<br/>
    /// 2. 配置結果から各アイテムの座標とサイズを取得する<br/>
    /// 3. 各アイテムを配置する<br/>
    /// </remarks>
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count == 0 || _layout is null)
        {
            return finalSize;
        }

        var childElements = ChildElements;
        var itemLayouts = _layout.GetLastItemLayouts();
        Debug.WriteLine($"LayoutColumnSegments: {LayoutColumnSegments}");
        Debug.WriteLine($"Layout.Y: [{string.Join(",", itemLayouts.Select(s => s.Y))}]");

        int layoutIndex = 0;
        foreach (var layout in itemLayouts)
        {
            if (layoutIndex >= childElements.Count)
            {
                break;
            }

            childElements[layoutIndex].Arrange(new Rect(layout.X, layout.Y, layout.Width, layout.Height));
            layoutIndex++;
        }

        return finalSize;
    }

    /// <summary>配置対象の子要素を取得する。</summary>
    /// <value>子要素のうち FrameworkElement であるもののリスト。</value>
    private List<FrameworkElement> ChildElements => [.. Children.OfType<FrameworkElement>()];
}
