using ClipboardZenHanConverter.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardZenHanConverter.Presentation.ViewModels;

/// <summary>文字列置換ペアの編集項目です。</summary>
/// <remarks>編集内容の変更は <see cref="ObservableObject"/> の変更通知で <see cref="SettingsViewModel"/> へ伝わり、
/// 検証と設定への反映が自動で行われます。</remarks>
public sealed partial class ReplacePairItem : ObservableObject
{
    /// <summary>検索文字列を取得または設定します（空不可）。</summary>
    [ObservableProperty]
    public partial string Search { get; set; } = string.Empty;

    /// <summary>置換文字列を取得または設定します（空不可）。</summary>
    [ObservableProperty]
    public partial string Replace { get; set; } = string.Empty;

    /// <summary>正規表現を使用するかどうかを取得または設定します。</summary>
    [ObservableProperty]
    public partial bool IsRegex { get; set; }

    /// <summary>バリデーションエラーメッセージを取得または設定します。null ならエラーなし。</summary>
    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    /// <summary>空の ReplacePairItem を作成します。</summary>
    public ReplacePairItem()
    {
    }

    /// <summary>ReplacePair から値をコピーして ReplacePairItem を作成します。</summary>
    /// <param name="pair">コピー元の置換ペア。</param>
    public ReplacePairItem(ReplacePair pair)
    {
        Search = pair.Search;
        Replace = pair.Replace;
        IsRegex = pair.IsRegex;
    }

    /// <summary>バリデーションを実行し、エラーメッセージを返します。正常時は null。</summary>
    /// <returns>エラーメッセージ。バリデーション成功時は null。</returns>
    /// <remarks>検証ロジックは <see cref="ReplacePair.TryValidate"/> に委譲します。</remarks>
    public string? Validate()
    {
        if (!ReplacePair.TryValidate(Search, Replace, IsRegex, out var error))
        {
            return ErrorMessage = error;
        }

        ErrorMessage = null;
        return null;
    }

    /// <summary>現在の値を ReplacePair に変換します。</summary>
    /// <returns>現在の設定を反映した新しい ReplacePair インスタンス。</returns>
    /// <remarks>変換前に Validate の呼び出しが必要です。<br/>
    /// 未検証のデータで ToPair を呼び出すと不正な ReplacePair が生成される可能性があります。</remarks>
    public ReplacePair ToPair() => new(Search, Replace, IsRegex);
}
