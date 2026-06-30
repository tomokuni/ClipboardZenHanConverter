using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardZenHanConverter.Core.Models;

/// <summary>アプリケーション全般の設定を保持するクラスです。</summary>
/// <remarks>
/// ウィンドウサイズなどのUIの状態や、アプリの動作設定を管理します。<br/>
/// </remarks>
public partial class AppSetting : ObservableObject, IDisposable
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

    // Debounce用の CancellationTokenSource
    [JsonIgnore]
    private CancellationTokenSource? _debounceCts;

    // ファイル書き込み中の排他制御用
    [JsonIgnore]
    private readonly SemaphoreSlim _saveLock = new(1, 1);


    public AppSetting()
    {
        // プロパティ変更通知の購読
        this.PropertyChanged += OnAnyPropertyChanged;
    }

    private void OnAnyPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        OnSettingsChanged();
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

    /// <summary>保留中の Debounce 保存をキャンセルします。</summary>
    public void CancelPendingSave()
    {
        _debounceCts?.Cancel();
    }

    private void OnSettingsChanged()
    {
        if (!IsAutoSave)
            return;

        // 既存の Debounce をキャンセル
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        // 300ms の Debounce 後に保存を実行
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(300, token);

                // キャンセルされていなければ実際に保存
                await SaveCoreAsync(AutoSaveFileName);
            }
            catch (OperationCanceledException)
            {
                // Debounce 中に再度変更があった場合は何もしない
            }
        });
    }

    private async Task SaveCoreAsync(string filePath)
    {
        await _saveLock.WaitAsync();
        try
        {
            await SaveToJsonFileAsync(filePath);
        }
        finally
        {
            _saveLock.Release();
        }
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

        using var stream = File.OpenRead(filePath);
        var loaded = JsonSerializer.Deserialize<AppSetting>(stream, AppJsonContext.Default.AppSetting);
        this.ApplyFrom(loaded);
    }

    public void ApplyFrom(AppSetting? other)
    {
        if (other == null)
            return;

        this.WindowWidth = other.WindowWidth;
        this.WindowHeight = other.WindowHeight;
    }

    public void Dispose()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = null;
        _saveLock.Dispose();
    }


}

