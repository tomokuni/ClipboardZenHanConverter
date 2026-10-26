namespace EsUtil.ZenHanConverter.App.Core.Models;

/// <summary>UI セグメントコントロールの個別アイテムを表します。</summary>
/// <param name="Content">表示テキスト</param>
/// <param name="Value">対応する列挙値</param>
/// <param name="IsEnabled">このアイテムが有効かどうか</param>
public sealed record SegmentItem(string Content, object Value, bool IsEnabled = true);

