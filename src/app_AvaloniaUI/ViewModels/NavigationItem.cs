using Avalonia.Media;

namespace ClipboardZenHanConverter.App.AvaloniaUI.ViewModels;

/// <summary>ナビゲーションペインの項目を表します。</summary>
/// <param name="Tag">ページタグ（"Home" / "Settings"）。</param>
/// <param name="Label">表示ラベル。</param>
/// <param name="Icon">項目アイコンの形状。</param>
public sealed record NavigationItem(string Tag, string Label, Geometry Icon);
