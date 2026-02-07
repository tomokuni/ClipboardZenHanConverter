using System;
using ClipboardZenHanConverter.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ClipboardZenHanConverter.Core.Helpers;

/// <summary>リストアイテムの有効・無効状態に応じてスタイルを切り替えるセレクタークラスです。</summary>
/// <remarks>
/// 提供機能として、SegmentItem の有効状態に応じたスタイル選択を行います。<br/>
/// 実装の詳細として、SegmentItem の IsEnabled を評価して EnabledStyle または DisabledStyle を返します。<br/>
/// 注意点として、EnabledStyle と DisabledStyle は事前に設定されている必要があります。<br/>
/// 最適化施策として、switch 式で分岐を簡潔化して不要な処理を削減します。<br/>
/// </remarks>
public partial class EnableStyleSelector : StyleSelector
{
    public Style? EnabledStyle { get; set; }
    public Style? DisabledStyle { get; set; }

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
