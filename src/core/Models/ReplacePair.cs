using System;
using System.Text.RegularExpressions;

namespace EsUtil.ClipboardZenHanConverter.Core.Models;

/// <summary>ユーザー定義の文字列置換ペアを表します。</summary>
/// <param name="Search">置換元の文字列（正規表現可）</param>
/// <param name="Replace">置換先の文字列</param>
/// <param name="IsRegex">正規表現として扱う場合は true</param>
/// <remarks>値の検証は <see cref="TryValidate"/> を使用します。<br/>
/// 空の検索文字列や不正な正規表現パターンは検証エラーとなります。</remarks>
public record ReplacePair(string Search, string Replace, bool IsRegex = false)
{
    /// <summary>指定された置換パラメーターを検証します。</summary>
    /// <param name="search">検索文字列</param>
    /// <param name="replace">置換文字列</param>
    /// <param name="isRegex">正規表現を使用するかどうか</param>
    /// <param name="errorMessage">検証エラーメッセージ。成功時は null。</param>
    /// <returns>検証に成功した場合は true。</returns>
    /// <remarks>
    /// 検証ルール: <br/>
    /// - search は空でないこと<br/>
    /// - replace は空でないこと<br/>
    /// - isRegex が true の場合、search は有効な正規表現であること</remarks>
    public static bool TryValidate(string search, string replace, bool isRegex, out string? errorMessage)
    {
        if (string.IsNullOrEmpty(search))
        {
            errorMessage = "検索文字列は必須です";
            return false;
        }

        if (string.IsNullOrEmpty(replace))
        {
            errorMessage = "置換文字列は必須です";
            return false;
        }

        if (isRegex)
        {
            try
            {
                _ = new Regex(search);
            }
            catch (ArgumentException)
            {
                // ユーザー入力の正規表現が無効な場合の検証エラー。
                errorMessage = "正規表現の形式が正しくありません";
                return false;
            }
        }

        errorMessage = null;
        return true;
    }
}
