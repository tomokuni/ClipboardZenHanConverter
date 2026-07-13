using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardZenHanConverter.Core.Models;

/// <summary>JSONファイルへの永続化機能を提供する設定クラスの基底クラスです。</summary>
/// <typeparam name="T">派生クラスの型</typeparam>
/// <remarks>
/// Debounce 保存、SemaphoreSlim による排他制御、JSON ファイル入出力を共通化します。<br/>
/// </remarks>
public abstract partial class SettingsPersistenceBase<T> : ObservableObject, IDisposable where T : class
{
    /// <summary>JSONシリアライズに使用する型情報を提供します。</summary>
    protected abstract (JsonSerializerContext Context, Type Type) SerializeInfo { get; }

    /// <summary>自動保存を有効にするかどうかを取得または設定します。</summary>
    [JsonIgnore]
    public bool IsAutoSave { get; set; }

    /// <summary>自動保存先のファイルパスを取得または設定します。</summary>
    [JsonIgnore]
    public string AutoSaveFileName { get; set; } = string.Empty;

    [JsonIgnore]
    private CancellationTokenSource? _debounceCts;

    [JsonIgnore]
    private readonly SemaphoreSlim _saveLock = new(1, 1);

    protected SettingsPersistenceBase()
    {
        PropertyChanged += OnAnyPropertyChanged;
    }

    /// <summary>設定を初期化します。ファイルから読み込み、自動保存を有効にします。</summary>
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

    /// <summary>保留中のDebounce保存をキャンセルします。</summary>
    public void CancelPendingSave()
    {
        _debounceCts?.Cancel();
    }

    private void OnAnyPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (!IsAutoSave) return;

        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(300, token);
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

    /// <summary>非同期でJSONファイルに保存します。</summary>
    public async Task SaveToJsonFileAsync(string filePath)
    {
        try
        {
            var (ctx, type) = SerializeInfo;
            using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, this, type, ctx);
        }
        catch
        {
            // ファイル保存に失敗してもアプリには影響させない
        }
    }

    /// <summary>同期的にJSONファイルに保存します。</summary>
    public void SaveToJsonFile(string filePath)
    {
        try
        {
            var (ctx, type) = SerializeInfo;
            using var stream = File.Create(filePath);
            JsonSerializer.Serialize(stream, this, type, ctx);
        }
        catch
        {
            // ファイル保存に失敗してもアプリには影響させない
        }
    }

    /// <summary>JSONファイルから設定を読み込みます。</summary>
    public void LoadFromJsonFile(string filePath)
    {
        if (!File.Exists(filePath)) return;

        try
        {
            var (ctx, type) = SerializeInfo;
            using var stream = File.OpenRead(filePath);
            var loaded = JsonSerializer.Deserialize(stream, type, ctx) as T;
            if (loaded is not null)
            {
                ApplyFrom(loaded);
            }
        }
        catch
        {
            // 読み込みに失敗しても無視
        }
    }

    /// <summary>他のインスタンスの値を自身に適用します。</summary>
    /// <param name="other">適用元のインスタンス</param>
    protected abstract void ApplyFrom(T other);

    public void Dispose()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = null;
        _saveLock.Dispose();
    }
}
