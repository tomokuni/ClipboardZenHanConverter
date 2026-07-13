using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardZenHanConverter.Core.Models;

/// <summary>アプリケーションのウィンドウ設定を管理します。</summary>
/// <remarks>ウィンドウの位置・サイズ情報を JSON ファイルに自動保存・復元します。<br/>
/// 保存先: %LOCALAPPDATA%\ClipboardZenHanConverter\AppSetting.json<br/>
/// 最適化手法: <br/>
/// - SettingsPersistenceBase によるデバウンス付き自動保存（300ms）<br/>
/// - System.Text.Json ソースジェネレーターによる高速シリアライズ</remarks>
public partial class AppSetting : SettingsPersistenceBase<AppSetting>
{
    /// <summary>ウィンドウの幅（DIP 非依存の論理ピクセル値）。</summary>
    [ObservableProperty]
    public partial double WindowWidth { get; set; } = 1000;

    /// <summary>ウィンドウの高さ（DIP 非依存の論理ピクセル値）。</summary>
    [ObservableProperty]
    public partial double WindowHeight { get; set; } = 800;

    /// <summary>シリアライズに使用する JsonSerializerContext と型を取得します。</summary>
    protected override SerializableTypeInfo SerializeInfo
        => new(AppJsonContext.Default, typeof(AppSetting));

    /// <summary>AppSetting の新しいインスタンスを初期化します。自動保存先を %LOCALAPPDATA% 配下に設定します。</summary>
    public AppSetting()
    {
        AutoSaveFileName = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ClipboardZenHanConverter",
            "AppSetting.json");
    }

    /// <summary>読み込んだ設定を現在のインスタンスに適用します。</summary>
    /// <param name="other">読み込んだ設定インスタンス。</param>
    protected override void ApplyFrom(AppSetting other)
    {
        WindowWidth = other.WindowWidth;
        WindowHeight = other.WindowHeight;
    }
}
