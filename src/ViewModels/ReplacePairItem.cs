using System.Text.RegularExpressions;
using ClipboardZenHanConverter.Core.Models;

namespace ClipboardZenHanConverter.ViewModels;

/// <summary>文字列置換ペアの編集項目です（PropertyChanged なしの単純 record）。</summary>
public record ReplacePairItem
{
    /// <summary>検索文字列（空欄不可）。</summary>
    public string Search { get; set; } = string.Empty;

    /// <summary>置換文字列（空欄不可）。</summary>
    public string Replace { get; set; } = string.Empty;

    /// <summary>正規表現を使用するかどうか。</summary>
    public bool IsRegex { get; set; }

    /// <summary>バリデーションエラーメッセージ。null ならエラーなし。</summary>
    public string? ErrorMessage { get; set; }

    public ReplacePairItem() { }

    public ReplacePairItem(ReplacePair pair)
    {
        Search = pair.Search;
        Replace = pair.Replace;
        IsRegex = pair.IsRegex;
    }

    /// <summary>バリデーションを実行し、エラーメッセージを返します。正常時は null。</summary>
    public string? Validate()
    {
        if (string.IsNullOrEmpty(Search))
            return ErrorMessage = "検索文字列は必須です";

        if (string.IsNullOrEmpty(Replace))
            return ErrorMessage = "置換文字列は必須です";

        if (IsRegex)
        {
            try
            {
                _ = new Regex(Search);
            }
            catch (RegexParseException)
            {
                return ErrorMessage = "正規表現の形式が正しくありません";
            }
        }

        ErrorMessage = null;
        return null;
    }

    /// <summary>現在の値を ReplacePair に変換します。</summary>
    public ReplacePair ToPair() => new(Search, Replace, IsRegex);
}
