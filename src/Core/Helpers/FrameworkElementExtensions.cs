using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace ClipboardZenHanConverter.Core.Helpers;

/// <summary>
/// FrameworkElement に対するレイアウト関連の拡張メソッド群。
/// 可読性・簡潔さ・モダンなC#記法を優先し、幅計算の汎用性を重視。
/// </summary>
public static class FrameworkElementExtensions
{
    /// <summary>
    /// 要素の内側サイズ（ボーダーとパディングを除いたサイズ）を取得します。<br/>
    /// ActualWidth と ActualHeight からボーダー厚さとパディングを差し引いたサイズを計算します。
    /// </summary>
    /// <param name="element">対象の FrameworkElement。</param>
    /// <returns>内側サイズ。無効な場合はデフォルトサイズ。</returns>
    public static Size GetInnerSize(this FrameworkElement element)
    {
        try
        {
            if (element is null ||
                !element.ActualWidth.IsValidWidth() ||
                !element.ActualHeight.IsValidWidth())
            {
                return default;
            }

            var border = element.GetBorderThickness();
            var padding = element.GetPadding();
            var consumption = new Thickness(border.Left + padding.Left, border.Top + padding.Top, border.Right + padding.Right, border.Bottom + padding.Bottom); ;
            double resultWidth = element.ActualWidth - (consumption.Left + consumption.Right);
            double resultHeight = element.ActualHeight - (consumption.Top + consumption.Bottom);

            return new Size(
                Math.Max(0, resultWidth),
                Math.Max(0, resultHeight)
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetInnerSize: Exception occurred - {ex.Message}");
            return default;
        }
    }

    /// <summary>
    /// 親要素の使用可能サイズ（Margin, Padding, BorderThickness を除いたサイズ）を取得します。<br/>
    /// 親要素が存在し、サイズが有効な場合、消費量を差し引いたサイズを返します。<br/>
    /// 例外が発生した場合はログを出力し、デフォルトサイズを返します。
    /// </summary>
    /// <param name="element">対象の FrameworkElement</param>
    /// <param name="consumptionSize">消費サイズ。省略時は GetConsumptionSize を使用。</param>
    /// <returns>親の使用可能サイズ。親が存在しないか無効な場合はデフォルトサイズ。</returns>
    public static Size GetParentUsableSize(this FrameworkElement element, Thickness? consumptionSize = null)
    {
        try
        {
            if (element?.Parent is not FrameworkElement parent ||
                !parent.ActualWidth.IsValidWidth() ||
                !parent.ActualHeight.IsValidWidth())
            {
                return default;
            }

            var consumption = consumptionSize ?? GetConsumptionSize(parent);
            double resultWidth = parent.ActualWidth - (consumption.Left + consumption.Right);
            double resultHeight = parent.ActualHeight - (consumption.Top + consumption.Bottom);

            return new Size(
                Math.Max(0, resultWidth),
                Math.Max(0, resultHeight)
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetUsableSize: Exception occurred - {ex.Message}");
            return default;
        }
    }

    /// <summary>
    /// Margin, Padding, BorderThickness など各要素のサイズ消費量を合算します。<br/>
    /// これらのプロパティの合計を Thickness として返し、レイアウト計算で使用します。<br/>
    /// 必要に応じて拡張ポイントを追加可能。
    /// </summary>
    /// <param name="e">調査対象の FrameworkElement</param>
    /// <returns>サイズ消費量 (Thickness)</returns>
    public static Thickness GetConsumptionSize(this FrameworkElement e)
    {
        var border = e.GetBorderThickness();
        var padding = e.GetPadding();
        return new Thickness(
            e.Margin.Left + padding.Left + border.Left,
            e.Margin.Top + padding.Top + border.Top,
            e.Margin.Right + padding.Right + border.Right,
            e.Margin.Bottom + padding.Bottom + border.Bottom
        );
    }

    /// <summary>
    /// 要素の BorderThickness によるサイズ消費量を取得します。
    /// </summary>
    /// <param name="e">調査対象の FrameworkElement</param>
    /// <returns>BorderThickness (Thickness)</returns>
    public static Thickness GetBorderThickness(this FrameworkElement e) => e switch
    {
        Border b => b.BorderThickness,
        _ => default,
    };

    /// <summary>
    /// 要素の Padding によるサイズ消費量を取得します。
    /// </summary>
    /// <param name="e">調査対象の FrameworkElement</param>
    /// <returns>Padding (Thickness)</returns>
    public static Thickness GetPadding(this FrameworkElement e) => e switch
    {
        Control c => c.Padding,
        Border b => b.Padding,
        _ => default
    };

    /// <summary>
    /// 指定された幅が有効な値かどうかを判定します。<br/>
    /// 幅が0以上の有限数であり、NaN や無限大でないことを確認します。
    /// </summary>
    /// <param name="width">判定する幅。</param>
    /// <returns>幅が有効であれば true、それ以外は false。</returns>
    public static bool IsValidWidth(this double width)
    {
        return width >= 0 && !double.IsNaN(width) && !double.IsInfinity(width);
    }
}
