using System.Text.RegularExpressions;
using ClipboardZenHanConverter.Core.Models;

namespace ClipboardZenHanConverter.App.ViewModels;

/// <summary>文字列置換ペアの編集項目です。</summary>
public sealed class ReplacePairItem
{
    /// <summary>検索文字列を取得または設定します（空不可）。</summary>
    public string Search { get; set; } = string.Empty;

    /// <summary>置換文字列を取得または設定します（空不可）。</summary>
    public string Replace { get; set; } = string.Empty;

    /// <summary>正規表現を使用するかどうかを取得または設定します。</summary>
    public bool IsRegex { get; set; }

    /// <summary>バリデーションエラーメッセージを取得または設定します。null ならエラーなし。</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>空の ReplacePairItem を作成します。</summary>
    public ReplacePairItem() { }

    /// <summary>ReplacePair から値をコピーして ReplacePairItem を作成します。</summary>
    /// <param name="pair">コピー元の置換ペア。</param>
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
    /// <returns>現在の設定を反映した新しい ReplacePair インスタンス。</returns>
    public ReplacePair ToPair() => new(Search, Replace, IsRegex);
}
