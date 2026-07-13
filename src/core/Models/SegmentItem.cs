namespace ClipboardZenHanConverter.Core.Models;

public record SegmentItem(string Content, object Value, bool IsEnabled = true);
public record SegmentDefine(string Label, string Prop, double Height = double.NaN, SegmentItem[]? Segments = null, bool? ForceEnableState = null);
