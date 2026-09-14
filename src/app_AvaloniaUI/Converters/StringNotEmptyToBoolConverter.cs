using Avalonia.Data.Converters;
using System.Globalization;

namespace ClipboardZenHanConverter.App.AvaloniaUI.Converters;

/// <summary>文字列が空でない場合に true を返す値コンバーター。</summary>
/// <remarks>任意項目（見出し・説明・エラーメッセージなど）の表示可否を IsVisible へバインドするために使用します。</remarks>
public sealed class StringNotEmptyToBoolConverter : IValueConverter
{
    /// <summary>共有インスタンスを取得します。</summary>
    /// <value>XAML から再利用できるステートレスなインスタンス。</value>
    public static readonly StringNotEmptyToBoolConverter Instance = new();

    /// <summary>文字列が空でないかどうかを bool へ変換します。</summary>
    /// <param name="value">判定する値（文字列以外は false 扱い）。</param>
    /// <param name="targetType">変換先の型。</param>
    /// <param name="parameter">使用しません。</param>
    /// <param name="culture">使用しません。</param>
    /// <returns>空でない場合は true。</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrEmpty(value as string);

    /// <summary>逆変換はサポートしません。</summary>
    /// <param name="value">使用しません。</param>
    /// <param name="targetType">使用しません。</param>
    /// <param name="parameter">使用しません。</param>
    /// <param name="culture">使用しません。</param>
    /// <returns>常に例外をスローします。</returns>
    /// <exception cref="NotSupportedException">常にスローされます。</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
