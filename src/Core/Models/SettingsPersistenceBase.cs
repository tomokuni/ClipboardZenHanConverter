using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardZenHanConverter.Core.Models;

/// <summary>設定のJSONファイルへの自動永続化を提供する基底クラス。</summary>
/// <remarks>
/// プロパティ変更から300msのデバウンスで自動保存します。<br/>
/// デバウンス中にさらに変更があった場合は、タイマーがリセットされます。<br/>
/// 保存は CancellationTokenSource によるキャンセル制御付きの非同期パターンで行われます。<br/><br/>
/// 最適化施策: <br/>
/// - CancellationTokenSource によるデバウンス（不要なファイル書き込みを抑制）<br/>
/// - await Task.Delay による非同期待機（UIスレッドをブロックしない）<br/>
/// - GC.SuppressFinalize によるファイナライズ抑制</remarks>
/// <typeparam name="T">永続化対象の具象型。</typeparam>
public abstract partial class SettingsPersistenceBase<T> : ObservableObject, IDisposable where T : class
{
    /// <summary>シリアライズに使用する JsonSerializerContext と型を取得します。</summary>
    protected abstract (JsonSerializerContext Context, Type Type) SerializeInfo { get; }

    /// <summary>自動保存が有効かどうかを取得または設定します。</summary>
    [JsonIgnore]
    public bool IsAutoSave { get; set; }

    /// <summary>自動保存先のファイルパスを取得または設定します。</summary>
    [JsonIgnore]
    public string AutoSaveFileName { get; set; } = string.Empty;

    /// <summary>デバウンスのキャンセルトークンソース。プロパティ変更のたびに新規作成されます。</summary>
    [JsonIgnore]
    private CancellationTokenSource? _debounceCts;
    /// <summary>Dispose 済みフラグ。</summary>
    private bool _disposed;

    /// <summary>SettingsPersistenceBase の新しいインスタンスを初期化します。PropertyChanged イベントの監視を開始します。</summary>
    protected SettingsPersistenceBase()
    {
        PropertyChanged += OnAnyPropertyChanged;
    }

    /// <summary>設定ファイルを読み込み、自動保存を開始します。</summary>
    public void Initialize()
    {
        var dir = Path.GetDirectoryName(AutoSaveFileName);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        LoadFromJsonFile(AutoSaveFileName);
        IsAutoSave = true;
    }

    /// <summary>保留中の自動保存をキャンセルします。</summary>
    public void CancelPendingSave()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = null;
    }

    /// <summary>プロパティ変更時に呼び出され、300ms のデバウンスで自動保存をスケジュールします。</summary>
    /// <remarks>デバウンス中にさらにプロパティが変更された場合、前回の保存予定はキャンセルされ、再度300msからカウントが始まります。</remarks>
    private void OnAnyPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (!IsAutoSave)
            return;

        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = new CancellationTokenSource();
        var ct = _debounceCts.Token;

        _ = SaveWithDebounceAsync(ct);
    }

    /// <summary>300msのデバウンス後に非同期で設定ファイルを保存します。</summary>
    /// <param name="ct">キャンセルトークン。</param>
    /// <remarks>デバウンス中にキャンセルされた場合は保存を行いません。</remarks>
    private async Task SaveWithDebounceAsync(CancellationToken ct)
    {
        try
        {
            await Task.Delay(300, ct);
            await SaveToJsonFileAsync(AutoSaveFileName, ct);
        }
        catch (OperationCanceledException)
        {
            // デバウンス中に別のプロパティ変更があった場合の正常なキャンセル。
        }
    }

    /// <summary>現在の設定を JSON ファイルに非同期で保存します。</summary>
    /// <param name="filePath">保存先のファイルパス。</param>
    /// <param name="ct">キャンセルトークン。</param>
    /// <remarks>
    /// ファイルの書き込み中に例外が発生した場合でもアプリケーションの動作には影響しません。<br/>
    /// 自動保存はベストエフォートであり、設定が失われても手動で再設定可能です。</remarks>
    public async Task SaveToJsonFileAsync(string filePath, CancellationToken ct = default)
    {
        try
        {
            var (ctx, type) = SerializeInfo;
            await using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, this, type, ctx, ct);
            await stream.FlushAsync(ct);
        }
        catch (OperationCanceledException)
        {
            // キャンセルによる中断は正常動作。
        }
        catch
        {
            // ファイル書き込み権限不足やディスク容量不足などのI/Oエラーは
            // アプリケーションの動作に影響を与えないよう握り潰す。
        }
    }

    /// <summary>現在の設定を JSON ファイルに同期的に保存します。</summary>
    /// <param name="filePath">保存先のファイルパス。</param>
    /// <remarks>同期的な保存が必要な場合（エクスポート等）に使用します。</remarks>
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
            // ファイル書き込み失敗時もアプリケーションの動作は継続する。
        }
    }

    /// <summary>JSON ファイルから設定を読み込み、適用します。</summary>
    /// <param name="filePath">読み込み元のファイルパス。</param>
    /// <remarks>ファイルが存在しない場合は何も行いません。JSON が不正な場合も無視して現在の設定を維持します。</remarks>
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
        catch
        {
            // JSON デシリアライズ失敗時も現在の設定を維持する。
        }
    }

    /// <summary>読み込んだ設定を現在のインスタンスに適用します。</summary>
    /// <param name="other">読み込んだ設定オブジェクト。</param>
    protected abstract void ApplyFrom(T other);

    /// <summary>リソースを解放します。デバウンスのキャンセルトークンソースを破棄します。</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
