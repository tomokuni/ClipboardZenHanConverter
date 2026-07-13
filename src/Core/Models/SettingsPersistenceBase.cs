using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardZenHanConverter.Core.Models;

public abstract partial class SettingsPersistenceBase<T> : ObservableObject, IDisposable where T : class
{
    protected abstract (JsonSerializerContext Context, Type Type) SerializeInfo { get; }

    [JsonIgnore]
    public bool IsAutoSave { get; set; }

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

    public void Initialize()
    {
        var dir = Path.GetDirectoryName(AutoSaveFileName);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        LoadFromJsonFile(AutoSaveFileName);
        IsAutoSave = true;
    }

    public void CancelPendingSave() => _debounceCts?.Cancel();

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
            catch (OperationCanceledException) { }
        });
    }

    private async Task SaveCoreAsync(string filePath)
    {
        await _saveLock.WaitAsync();
        try { await SaveToJsonFileAsync(filePath); }
        finally { _saveLock.Release(); }
    }

    public async Task SaveToJsonFileAsync(string filePath)
    {
        try
        {
            var (ctx, type) = SerializeInfo;
            using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, this, type, ctx);
        }
        catch { }
    }

    public void SaveToJsonFile(string filePath)
    {
        try
        {
            var (ctx, type) = SerializeInfo;
            using var stream = File.Create(filePath);
            JsonSerializer.Serialize(stream, this, type, ctx);
        }
        catch { }
    }

    public void LoadFromJsonFile(string filePath)
    {
        if (!File.Exists(filePath)) return;
        try
        {
            var (ctx, type) = SerializeInfo;
            using var stream = File.OpenRead(filePath);
            if (JsonSerializer.Deserialize(stream, type, ctx) is T loaded)
                ApplyFrom(loaded);
        }
        catch { }
    }

    protected abstract void ApplyFrom(T other);

    public void Dispose()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = null;
        _saveLock.Dispose();
    }
}
