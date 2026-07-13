using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace ClipboardZenHanConverter.ViewModels.Converters;

/// <summary>空文字列でない場合に Visible にするコンバーターです。</summary>
public partial class StringNotEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string? language)
        => string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, string? language)
        => throw new NotSupportedException();
}
