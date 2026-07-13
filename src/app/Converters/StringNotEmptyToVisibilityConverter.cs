using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace ClipboardZenHanConverter.App.Converters;

/// <summary>空文字列でない場合に Visible にするコンバーターです。</summary>
/// <remarks>文字列が null または空の場合は Collapsed、それ以外は Visible を返します。<br/>
/// 設定画面のエラーメッセージ表示などに使用します。</remarks>
public sealed partial class StringNotEmptyToVisibilityConverter : IValueConverter
{
    /// <summary>文字列の値から Visibility に変換します。</summary>
    /// <param name="value">変換する文字列値。</param>
    /// <param name="targetType">未使用。</param>
    /// <param name="parameter">未使用。</param>
    /// <param name="language">未使用。</param>
    /// <returns>空文字列の場合は Visibility.Collapsed、それ以外は Visibility.Visible。</returns>
    public object Convert(object? value, Type targetType, object? parameter, string? language)
        => string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>逆変換。常に NotSupportedException をスローします。</summary>
    /// <exception cref="NotSupportedException">逆変換はサポートされていません。</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, string? language)
        => throw new NotSupportedException();
}
