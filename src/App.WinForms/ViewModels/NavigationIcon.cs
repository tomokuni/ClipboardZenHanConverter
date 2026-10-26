namespace EsUtil.ZenHanConverter.App.WinForms.ViewModels;

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
