using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.ComponentModel;
using ClipboardZenHanConverter.Core.Enums;


namespace ClipboardZenHanConverter.Core.Models;

// ToDo: 文字列のリプレース
// ToDo: 設定のインポート/エクスポート、プリセット保存/読み込み
// ToDo: 半角の記号は一部を除き使用できません。使用可能な記号 ( ) ― / , . 半角スペースの7つ。
// ToDo: タブ,カンマで区切られたテキストからMarkDownのtable形式に変換
// ToDo: 連続スペースの除去
// ToDo: 半角スペースを _ に置き換え、半角 _ をスペースに置き換え

/// <summary>変換設定を保持するクラスです。</summary>
/// <remarks>
/// 各種文字種の変換モード（全角・半角・変換なし等）をプロパティとして管理します。<br/>
/// ObservableObject を継承しており、プロパティ変更通知を行います。<br/>
/// </remarks>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public partial class ConvertConfig : ObservableObject, IDisposable
{

    /// <summary>全角半角変換機能の有効状態を取得または設定します。</summary>
    [ObservableProperty]
    public partial bool IsEnabledZenHan { get; set; } = false;


    // 数字の変換設定
    /// <summary>数字の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeNumber { get; set; } = ZenHanMode.None;


    // 英字の変換設定
    [ObservableProperty]
    public partial ZenHanMode ConvertModeAlphabet { get; set; } = ZenHanMode.None;


    // Ascii記号の変換設定
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolParenthesis { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolSquareBracket { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolCurlyBracket { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolDoubleQuote { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolSingleQuote { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolComma { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolPeriod { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolColon { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolSemicolon { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolLessThan { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolEqual { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolGreaterThan { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolPlus { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolHyphenMinus { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolExclamation { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolSharp { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolDollar { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolPercent { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolAmpersand { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolAsterisk { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolSlash { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolQuestion { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolAt { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolCaret { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolUnderBar { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolBackquote { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolVerticalBar { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolTilde { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolSpace { get; set; } = ZenHanMode.None;


    // バックスラッシュの変換設定
    [ObservableProperty]
    public partial ZenHanMode ConvertModeZenbshHanbsh { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeZenyenHanbsh { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeZenyenHanyen { get; set; } = ZenHanMode.None;


    // カナの変換設定
    [ObservableProperty]
    public partial ZenHanKanaMode ConvertModeKanaHan { get; set; } = ZenHanKanaMode.None;

    [ObservableProperty]
    public partial ZenHanKanaMode ConvertModeKanaZenKata { get; set; } = ZenHanKanaMode.None;

    [ObservableProperty]
    public partial ZenHanKanaMode ConvertModeKanaZenHira { get; set; } = ZenHanKanaMode.None;


    // カナ記号（濁点、半濁点、中点、括弧）の変換設定
    [ObservableProperty]
    public partial ZenHanMode ConvertModeEtcKanaVoice { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeEtcKanaSemiVoice { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeEtcKanaMiddleDot { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeEtcKanaLeftCornerBracket { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeEtcKanaRightCornerBracket { get; set; } = ZenHanMode.None;


    // カナ記号（長音記号、区読点）の変換設定
    [ObservableProperty]
    public partial ZenHanEtcZenHanAsciiMode ConvertModeEtcKanaProlong { get; set; } = ZenHanEtcZenHanAsciiMode.None;

    [ObservableProperty]
    public partial ZenHanEtcZenHanAsciiMode ConvertModeEtcKanaPeriod { get; set; } = ZenHanEtcZenHanAsciiMode.None;

    [ObservableProperty]
    public partial ZenHanEtcZenHanAsciiMode ConvertModeEtcKanaComma { get; set; } = ZenHanEtcZenHanAsciiMode.None;


    // 記号（バックスラッシュ、円記号）の変換設定
    [ObservableProperty]
    public partial ZenHanEtcYenMode ConvertModeEtcBSlashHan { get; set; } = ZenHanEtcYenMode.None;

    [ObservableProperty]
    public partial ZenHanEtcYenMode ConvertModeEtcBSlashZen { get; set; } = ZenHanEtcYenMode.None;

    [ObservableProperty]
    public partial ZenHanEtcYenMode ConvertModeEtcYenHan { get; set; } = ZenHanEtcYenMode.None;

    [ObservableProperty]
    public partial ZenHanEtcYenMode ConvertModeEtcYenZen { get; set; } = ZenHanEtcYenMode.None;

    // タブの変換設定
    [ObservableProperty]
    public partial ZenHanEtcSpecial ConvertModeEtcTabSpace { get; set; } = ZenHanEtcSpecial.None;


    // 改行の変換設定
    [ObservableProperty]
    public partial ZenHanEtcSpecial ConvertModeEtcNewline { get; set; } = ZenHanEtcSpecial.None;


    // 連続スペースの変換設定
    [ObservableProperty]
    public partial ZenHanEtcSpecial ConvertModeEtcMultiSpace { get; set; } = ZenHanEtcSpecial.None;


    [JsonIgnore]
    public bool IsAutoSave { get; set; } = false;



    [JsonIgnore]
    public string AutoSaveFileName { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ClipboardZenHanConverter", "Settings.json");

    // Debounce用の CancellationTokenSource
    [JsonIgnore]
    private CancellationTokenSource? _debounceCts;

    // ファイル書き込み中の排他制御用
    [JsonIgnore]
    private readonly SemaphoreSlim _saveLock = new(1, 1);


    public ConvertConfig()
    {
        // プロパティ変更通知の購読
        this.PropertyChanged += OnAnyPropertyChanged;
    }

    private void OnAnyPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        OnSettingsChanged();
    }

    /// <summary>
    /// 設定の初期化を行います。ファイルから読み込み、自動保存を有効にします。
    /// </summary>
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


    /// <summary>
    /// このインスタンスをJSONとして指定ファイルに上書き保存します。
    /// </summary>
    public async Task SaveToJsonFileAsync(string filePath)
    {
        try
        {
            using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, this, AppJsonContext.Default.ConvertConfig);
        }
        catch { }
    }

    /// <summary>
    /// このインスタンスをJSONとして指定ファイルに同期的に上書き保存します。
    /// </summary>
    public void SaveToJsonFile(string filePath)
    {
        try
        {
            using var stream = File.Create(filePath);
            JsonSerializer.Serialize(stream, this, AppJsonContext.Default.ConvertConfig);
        }
        catch { }
    }


    public void LoadFromJsonFile(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        using var stream = File.OpenRead(filePath);
        var loaded = JsonSerializer.Deserialize<ConvertConfig>(stream, AppJsonContext.Default.ConvertConfig);
        this.ApplyFrom(loaded);
    }

    /// <summary>
    /// 使用中のリソースを解放します。
    /// </summary>
    public void Dispose()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _saveLock.Dispose();
    }

    /// <summary>
    /// 他のSettingsModelインスタンスの値をリフレクションで自身にコピーします。
    /// </summary>
    public void ApplyFrom(ConvertConfig? other)
    {
        if (other == null)
            return;

        [UnconditionalSuppressMessage("Trimming", "IL2075:Select-String", Justification = "Class is annotated with DynamicallyAccessedMembers")]
        static void CopyProperties(ConvertConfig source, ConvertConfig target)
        {
            var type = typeof(ConvertConfig);
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.CanWrite && prop.CanRead && prop.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                {
                    var value = prop.GetValue(source);
                    prop.SetValue(target, value);
                }
            }
        }

        CopyProperties(other, this);
    }


}

