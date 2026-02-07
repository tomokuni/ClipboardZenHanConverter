using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardZenHanConverter.Core.Models;

/// <summary>アプリケーション全般の設定を保持するクラスです。</summary>
/// <remarks>
/// ウィンドウサイズなどのUIの状態や、アプリの動作設定を管理します。<br/>
/// </remarks>
public partial class AppSetting : ObservableObject
{
    /// <summary>ウィンドウの幅を取得または設定します。</summary>
    [ObservableProperty]
    public partial double WindowWidth { get; set; } = 1000;

    /// <summary>ウィンドウの高さを取得または設定します。</summary>
    [ObservableProperty]
    public partial double WindowHeight { get; set; } = 800;

    [JsonIgnore]
    public bool IsAutoSave { get; set; } = false;

    [JsonIgnore]
    public string AutoSaveFileName { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ClipboardZenHanConverter", "AppSetting.json");

    public AppSetting()
    {
        // プロパティ変更通知の購読
        this.PropertyChanged += (s, e) => OnSettingsChanged();
    }

    /// <summary>設定の初期化を行います。ファイルから読み込み、自動保存を有効にします。</summary>
    public void Initialize()
    {
        var dir = Path.GetDirectoryName(AutoSaveFileName);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        LoadFromJsonFile(AutoSaveFileName);
        IsAutoSave = true;
    }

    private void OnSettingsChanged()
    {
        if (IsAutoSave)
            _ = SaveToJsonFileAsync(AutoSaveFileName);
    }

    public async Task SaveToJsonFileAsync(string filePath)
    {
        try
        {
            using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, this, AppJsonContext.Default.AppSetting);
        }
        catch { }
    }

    public void SaveToJsonFile(string filePath)
    {
        try
        {
            using var stream = File.Create(filePath);
            JsonSerializer.Serialize(stream, this, AppJsonContext.Default.AppSetting);
        }
        catch { }
    }

    public void LoadFromJsonFile(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        try
        {
            using var stream = File.OpenRead(filePath);
            var loaded = JsonSerializer.Deserialize<AppSetting>(stream, AppJsonContext.Default.AppSetting);
            this.ApplyFrom(loaded);
        }
        catch { }
    }

    public void ApplyFrom(AppSetting? other)
    {
        if (other == null)
            return;

        this.WindowWidth = other.WindowWidth;
        this.WindowHeight = other.WindowHeight;
    }


}

