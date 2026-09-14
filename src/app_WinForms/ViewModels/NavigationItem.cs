namespace EsUtil.ClipboardZenHanConverter.App.WinForms.ViewModels;

/// <summary>アプリで使用する Fluent Icons のアイコン種別を表します。</summary>
/// <remarks>パスデータ（Core の FluentIconData）への対応は View 側の FluentIcons が担います。<br/>
/// この ViewModel 層が System.Drawing に依存しないよう、種別のみを保持します。</remarks>
public enum NavigationIcon
{
    /// <summary>変換範囲を表すアイコン（ホーム）。</summary>
    ConvertRange,

    /// <summary>歯車のアイコン（設定）。</summary>
    Settings,
}

/// <summary>ナビゲーションペインの項目を表します。</summary>
/// <param name="Tag">ページタグ（"Home" / "Settings"）。</param>
/// <param name="Label">表示ラベル。</param>
/// <param name="Icon">項目アイコンの種別。</param>
public sealed record NavigationItem(string Tag, string Label, NavigationIcon Icon);
