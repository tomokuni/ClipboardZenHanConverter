namespace ClipboardZenHanConverter.Core.Interfaces;

/// <summary>システムクリップボードの読み書きと内容変更監視を提供するインターフェース。</summary>
/// <remarks>DIP（依存性逆転の原則）に従い、抽象に依存するために定義されています。<br/>
/// 実装クラスはプラットフォーム固有のクリップボードAPIをラップします。<br/>
/// テスト時はモック/スタブに差し替え可能です。</remarks>
public interface IClipboardService : IDisposable
{
    /// <summary>クリップボード内容変更イベント。</summary>
    event EventHandler<object>? ContentChanged;

    /// <summary>クリップボードからテキストを非同期に取得します。</summary>
    /// <returns>取得したテキスト。取得できない場合は null</returns>
    Task<string?> GetTextAsync();

    /// <summary>指定されたテキストをクリップボードに設定します。</summary>
    /// <param name="text">設定するテキスト</param>
    void SetText(string text);

    /// <summary>クリップボードの内容をフラッシュ（永続化）します。</summary>
    void Flush();
}
