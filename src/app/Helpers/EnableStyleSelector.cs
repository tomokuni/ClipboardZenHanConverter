using ClipboardZenHanConverter.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ClipboardZenHanConverter.Helpers;

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
