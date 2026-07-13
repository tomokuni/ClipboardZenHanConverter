using ClipboardZenHanConverter.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ClipboardZenHanConverter.App.Helpers;

/// <summary>SegmentItem の IsEnabled 状態に応じてスタイルを切り替える StyleSelector。</summary>
/// <remarks>XAML の Segmented コントロール内で、有効/無効の各 SegmentItem に異なるスタイルを適用します。<br/>
/// EnabledStyle と DisabledStyle の両方が設定されている必要があります。</remarks>
public partial class EnableStyleSelector : StyleSelector
{
    /// <summary>有効状態のセグメントに適用するスタイルを取得または設定します。</summary>
    public Style? EnabledStyle { get; set; }

    /// <summary>無効状態のセグメントに適用するスタイルを取得または設定します。</summary>
    public Style? DisabledStyle { get; set; }

    /// <summary>アイテムの状態に応じてスタイルを選択します。</summary>
    /// <param name="item">スタイルを選択する対象アイテム</param>
    /// <param name="_">未使用の依存関係オブジェクト</param>
    /// <returns>選択されたスタイル</returns>
    protected override Style SelectStyleCore(object item, DependencyObject _)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(EnabledStyle);
        ArgumentNullException.ThrowIfNull(DisabledStyle);

        return item switch
        {
            SegmentItem { IsEnabled: true } => EnabledStyle,
            SegmentItem => DisabledStyle,
            _ => EnabledStyle,
        };
    }
}
