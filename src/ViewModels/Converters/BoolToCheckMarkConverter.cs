using System;
using Microsoft.UI.Xaml.Data;

namespace ClipboardZenHanConverter.ViewModels.Converters;

/// <summary>bool 値をチェックマーク文字列に変換します。</summary>
public partial class BoolToCheckMarkConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string? language)
        => (value is true) ? "✅" : "□";

    public object ConvertBack(object? value, Type targetType, object? parameter, string? language)
        => throw new NotSupportedException();
}
