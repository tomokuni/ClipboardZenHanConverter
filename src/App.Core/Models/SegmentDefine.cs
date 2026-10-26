namespace EsUtil.ZenHanConverter.App.Core.Models;

/// <summary>UI セグメントコントロールの定義を表します。</summary>
/// <param name="Label">ラベルテキスト。</param>
/// <param name="Mode">バインド先の型安全なモード定義（<see cref="ConvertConfig"/> の Get/Set アクセサ）。</param>
/// <param name="Height">コントロールの高さ（既定: NaN = 自動）。</param>
/// <param name="Segments">セグメント アイテムの配列（null の場合は既定のセグメントを使う）。</param>
/// <param name="ForceEnableState">有効/無効を強制する値（null の場合は自動判定）。</param>
public sealed record SegmentDefine(
    string Label,
    ModePropDef Mode,
    double Height = double.NaN,
    SegmentItem[]? Segments = null,
    bool? ForceEnableState = null);
