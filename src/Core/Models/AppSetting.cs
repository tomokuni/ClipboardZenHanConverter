using System;
using System.IO;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardZenHanConverter.Core.Models;

/// <summary>アプリケーション全般の設定を保持するクラスです。</summary>
/// <remarks>
/// ウィンドウサイズなどのUIの状態を管理し、JSONファイルに自動保存します。<br/>
/// </remarks>
public partial class AppSetting : SettingsPersistenceBase<AppSetting>
{
    /// <summary>ウィンドウの幅を取得または設定します。</summary>
    [ObservableProperty]
    public partial double WindowWidth { get; set; } = 1000;

    /// <summary>ウィンドウの高さを取得または設定します。</summary>
    [ObservableProperty]
    public partial double WindowHeight { get; set; } = 800;

    protected override (JsonSerializerContext Context, Type Type) SerializeInfo
        => (AppJsonContext.Default, typeof(AppSetting));

    public AppSetting()
    {
        AutoSaveFileName = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ClipboardZenHanConverter",
            "AppSetting.json");
    }

    protected override void ApplyFrom(AppSetting other)
    {
        WindowWidth = other.WindowWidth;
        WindowHeight = other.WindowHeight;
    }
}

