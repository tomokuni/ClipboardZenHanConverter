namespace ClipboardZenHanConverter.Core.Models;

/// <summary>UI セグメントコントロールの個別アイテムを表します。</summary>
/// <param name="Content">表示テキスト</param>
/// <param name="Value">対応する列挙値</param>
/// <param name="IsEnabled">このアイテムが有効かどうか</param>
public sealed record SegmentItem(string Content, object Value, bool IsEnabled = true);

/// <summary>UI セグメントコントロールの定義を表します。</summary>
/// <param name="Label">ラベルテキスト</param>
/// <param name="Prop">バインド先プロパティ名</param>
/// <param name="Height">コントロールの高さ（デフォルト: NaN = 自動）</param>
/// <param name="Segments">セグメントアイテム配列（null の場合はデフォルトセグメント使用）</param>
/// <param name="ForceEnableState">強制的に有効/無効状態を設定（null の場合は自動判定）</param>
public sealed record SegmentDefine(
    string Label,
    string Prop,
    double Height = double.NaN,
    SegmentItem[]? Segments = null,
    bool? ForceEnableState = null);
