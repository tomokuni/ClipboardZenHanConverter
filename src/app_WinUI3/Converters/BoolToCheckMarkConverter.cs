using Microsoft.UI.Xaml.Data;
using System;

namespace EsUtil.ClipboardZenHanConverter.App.WinUI.Converters;

/// <summary>bool 値をチェックマーク文字列に変換します。</summary>
/// <remarks>true の場合は "✅"、false の場合は "□" を返します。<br/>
/// 設定画面の置換ルール一覧で正規表現モード表示に使用します。</remarks>
public sealed partial class BoolToCheckMarkConverter : IValueConverter
{
    /// <summary>bool 値からチェックマーク文字列に変換します。</summary>
    /// <param name="value">変換する bool 値。</param>
    /// <param name="targetType">未使用。</param>
    /// <param name="parameter">未使用。</param>
    /// <param name="language">未使用。</param>
    /// <returns>true の場合は "✅"、false の場合は "□"。</returns>
    public object Convert(object? value, Type targetType, object? parameter, string? language)
        => (value is true) ? "✅" : "□";

    /// <summary>逆変換。常に NotSupportedException をスローします。</summary>
    /// <exception cref="NotSupportedException">逆変換はサポートされていません。</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, string? language)
        => throw new NotSupportedException();
}
