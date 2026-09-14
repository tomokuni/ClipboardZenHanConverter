namespace ClipboardZenHanConverter.Presentation.ViewModels;

/// <summary>ホーム画面で共有する表示文言を提供する静的クラス。</summary>
/// <remarks>4 つの UI の <c>HomeViewModel</c> が同じ文言を表示するための単一所有元です。<br/>
/// テストは仕様の固定のため、この定数ではなくリテラルで期待値を記述します。</remarks>
public static class HomeDisplayText
{
    /// <summary>変換結果が元テキストと同一の場合に表示する文言。</summary>
    /// <value>変換の必要がなかったことを利用者へ伝える表示文字列。</value>
    public const string NoChange = "変換不要 (変更なし)";
}
