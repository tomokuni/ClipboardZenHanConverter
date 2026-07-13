using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using ClipboardZenHanConverter.Core.Enums;

namespace ClipboardZenHanConverter.Core.Models;

// ToDo: 文字列のリプレース

/// <summary>変換設定を保持するクラスです。</summary>
/// <remarks>
/// 各種文字種の変換モード（全角・半角・変換なし等）をプロパティとして管理します。<br/>
/// プロパティ変更時は JSON ファイルに自動保存されます。<br/>
/// </remarks>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public partial class ConvertConfig : SettingsPersistenceBase<ConvertConfig>
{
    protected override (JsonSerializerContext Context, Type Type) SerializeInfo
        => (AppJsonContext.Default, typeof(ConvertConfig));

    public ConvertConfig()
    {
        AutoSaveFileName = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ClipboardZenHanConverter",
            "Settings.json");
    }

    protected override void ApplyFrom(ConvertConfig other)
    {
        [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Class is annotated with DynamicallyAccessedMembers")]
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

    // ====================================================================
    // 変換機能の有効/無効
    // ====================================================================

    /// <summary>全角半角変換機能の有効状態を取得または設定します。</summary>
    [ObservableProperty]
    public partial bool IsEnabledZenHan { get; set; }

    // ====================================================================
    // 数字・英字
    // ====================================================================

    [ObservableProperty]
    public partial ZenHanMode ConvertModeNumber { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeAlphabet { get; set; } = ZenHanMode.None;

    // ====================================================================
    // Ascii記号
    // ====================================================================

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

    // ====================================================================
    // カナ
    // ====================================================================

    [ObservableProperty]
    public partial ZenHanKanaMode ConvertModeKanaHan { get; set; } = ZenHanKanaMode.None;

    [ObservableProperty]
    public partial ZenHanKanaMode ConvertModeKanaZenKata { get; set; } = ZenHanKanaMode.None;

    [ObservableProperty]
    public partial ZenHanKanaMode ConvertModeKanaZenHira { get; set; } = ZenHanKanaMode.None;

    // ====================================================================
    // カナ記号（濁点、半濁点、中点、括弧）
    // ====================================================================

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

    // ====================================================================
    // カナ記号（長音記号、句読点）
    // ====================================================================

    [ObservableProperty]
    public partial ZenHanEtcZenHanAsciiMode ConvertModeEtcKanaProlong { get; set; } = ZenHanEtcZenHanAsciiMode.None;

    [ObservableProperty]
    public partial ZenHanEtcZenHanAsciiMode ConvertModeEtcKanaPeriod { get; set; } = ZenHanEtcZenHanAsciiMode.None;

    [ObservableProperty]
    public partial ZenHanEtcZenHanAsciiMode ConvertModeEtcKanaComma { get; set; } = ZenHanEtcZenHanAsciiMode.None;

    // ====================================================================
    // バックスラッシュ・円記号
    // ====================================================================

    [ObservableProperty]
    public partial ZenHanEtcYenMode ConvertModeEtcBSlashHan { get; set; } = ZenHanEtcYenMode.None;

    [ObservableProperty]
    public partial ZenHanEtcYenMode ConvertModeEtcBSlashZen { get; set; } = ZenHanEtcYenMode.None;

    [ObservableProperty]
    public partial ZenHanEtcYenMode ConvertModeEtcYenHan { get; set; } = ZenHanEtcYenMode.None;

    [ObservableProperty]
    public partial ZenHanEtcYenMode ConvertModeEtcYenZen { get; set; } = ZenHanEtcYenMode.None;

    // ====================================================================
    // タブ・改行・連続スペース
    // ====================================================================

    [ObservableProperty]
    public partial ZenHanEtcSpecial ConvertModeEtcTabSpace { get; set; } = ZenHanEtcSpecial.None;

    [ObservableProperty]
    public partial ZenHanEtcSpecial ConvertModeEtcNewline { get; set; } = ZenHanEtcSpecial.None;

    [ObservableProperty]
    public partial ZenHanEtcSpecial ConvertModeEtcMultiSpace { get; set; } = ZenHanEtcSpecial.None;

    // ====================================================================
    // 文字列置換
    // ====================================================================

    /// <summary>ユーザー定義の文字列置換ペア一覧（単純置換または正規表現）。</summary>
    public List<ReplacePair> ReplacePairs
    {
        get => _replacePairs;
        set
        {
            if (!ReferenceEquals(_replacePairs, value))
            {
                _replacePairs = value;
                OnPropertyChanged();
            }
        }
    }
    private List<ReplacePair> _replacePairs = [];

    // ====================================================================
    // 組込みプリセット
    // ====================================================================

    /// <summary>組込みプリセット名「全力会計」です。</summary>
    public const string BuiltInPresetAccountingPower = "全力会計（Built-in）";

    /// <summary>組込みプリセットの定義を取得します。</summary>
    private static readonly Dictionary<string, Action<ConvertConfig>> BuiltInPresets = new()
    {
        [BuiltInPresetAccountingPower] = ApplyAccountingPowerPreset,
    };

    /// <summary>組込みプリセット「全力会計」を適用します。</summary>
    private static void ApplyAccountingPowerPreset(ConvertConfig c)
    {
        c.IsEnabledZenHan = true;

        // 数字・英字 → 半角
        c.ConvertModeNumber = ZenHanMode.ToHan;
        c.ConvertModeAlphabet = ZenHanMode.ToHan;

        // ( ) ― / , . SP → 半角
        c.ConvertModeSymbolParenthesis = ZenHanMode.ToHan;
        c.ConvertModeSymbolHyphenMinus = ZenHanMode.ToHan;
        c.ConvertModeSymbolSlash = ZenHanMode.ToHan;
        c.ConvertModeSymbolComma = ZenHanMode.ToHan;
        c.ConvertModeSymbolPeriod = ZenHanMode.ToHan;
        c.ConvertModeSymbolSpace = ZenHanMode.ToHan;

        // それ以外の記号 → 全角
        c.ConvertModeSymbolSquareBracket = ZenHanMode.ToZen;
        c.ConvertModeSymbolCurlyBracket = ZenHanMode.ToZen;
        c.ConvertModeSymbolDoubleQuote = ZenHanMode.ToZen;
        c.ConvertModeSymbolSingleQuote = ZenHanMode.ToZen;
        c.ConvertModeSymbolColon = ZenHanMode.ToZen;
        c.ConvertModeSymbolSemicolon = ZenHanMode.ToZen;
        c.ConvertModeSymbolLessThan = ZenHanMode.ToZen;
        c.ConvertModeSymbolEqual = ZenHanMode.ToZen;
        c.ConvertModeSymbolGreaterThan = ZenHanMode.ToZen;
        c.ConvertModeSymbolPlus = ZenHanMode.ToZen;
        c.ConvertModeSymbolExclamation = ZenHanMode.ToZen;
        c.ConvertModeSymbolSharp = ZenHanMode.ToZen;
        c.ConvertModeSymbolDollar = ZenHanMode.ToZen;
        c.ConvertModeSymbolPercent = ZenHanMode.ToZen;
        c.ConvertModeSymbolAmpersand = ZenHanMode.ToZen;
        c.ConvertModeSymbolAsterisk = ZenHanMode.ToZen;
        c.ConvertModeSymbolQuestion = ZenHanMode.ToZen;
        c.ConvertModeSymbolAt = ZenHanMode.ToZen;
        c.ConvertModeSymbolCaret = ZenHanMode.ToZen;
        c.ConvertModeSymbolUnderBar = ZenHanMode.ToZen;
        c.ConvertModeSymbolBackquote = ZenHanMode.ToZen;
        c.ConvertModeSymbolVerticalBar = ZenHanMode.ToZen;
        c.ConvertModeSymbolTilde = ZenHanMode.ToZen;

        // カナ → 全角(カタカナ)
        c.ConvertModeKanaHan = ZenHanKanaMode.ToZenKata;
        c.ConvertModeKanaZenKata = ZenHanKanaMode.None;
        c.ConvertModeKanaZenHira = ZenHanKanaMode.None;

        // かな記号 → 全角
        c.ConvertModeEtcKanaVoice = ZenHanMode.ToZen;
        c.ConvertModeEtcKanaSemiVoice = ZenHanMode.ToZen;
        c.ConvertModeEtcKanaMiddleDot = ZenHanMode.ToZen;
        c.ConvertModeEtcKanaLeftCornerBracket = ZenHanMode.ToZen;
        c.ConvertModeEtcKanaRightCornerBracket = ZenHanMode.ToZen;

        // 長音記号 → ASCII, 読点・句点 → 半角
        c.ConvertModeEtcKanaProlong = ZenHanEtcZenHanAsciiMode.ToAscii;
        c.ConvertModeEtcKanaPeriod = ZenHanEtcZenHanAsciiMode.ToHan;
        c.ConvertModeEtcKanaComma = ZenHanEtcZenHanAsciiMode.ToHan;

        // バックスラッシュ・円記号
        //   半角バックスラッシュ → ¥, 全角バックスラッシュ → 変換なし
        //   半角円記号 → 変換なし, 全角円記号 → ¥
        c.ConvertModeEtcBSlashHan = ZenHanEtcYenMode.ToHanYen;
        c.ConvertModeEtcBSlashZen = ZenHanEtcYenMode.None;
        c.ConvertModeEtcYenHan = ZenHanEtcYenMode.None;
        c.ConvertModeEtcYenZen = ZenHanEtcYenMode.ToHanYen;

        // タブ・改行 → 半角SP
        c.ConvertModeEtcTabSpace = ZenHanEtcSpecial.ToHanSpace;
        c.ConvertModeEtcNewline = ZenHanEtcSpecial.ToHanSpace;

        // 連続SP → 単一半角SP
        c.ConvertModeEtcMultiSpace = ZenHanEtcSpecial.ToHanSpace;
    }

    // ====================================================================
    // インポート/エクスポート
    // ====================================================================

    /// <summary>設定をJSONファイルにエクスポートします。</summary>
    /// <param name="filePath">エクスポート先のファイルパス</param>
    public void ExportToFile(string filePath) => SaveToJsonFile(filePath);

    /// <summary>設定をJSONファイルからインポートします。</summary>
    /// <param name="filePath">インポート元のファイルパス</param>
    /// <returns>インポートに成功したかどうか</returns>
    public bool ImportFromFile(string filePath)
    {
        if (!File.Exists(filePath)) return false;

        try
        {
            var (ctx, type) = SerializeInfo;
            using var stream = File.OpenRead(filePath);
            if (JsonSerializer.Deserialize(stream, type, ctx) is not ConvertConfig loaded) return false;

            var wasAutoSave = IsAutoSave;
            IsAutoSave = false;
            ApplyFrom(loaded);
            IsAutoSave = wasAutoSave;

            SaveToJsonFile(AutoSaveFileName);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // ====================================================================
    // プリセット管理
    // ====================================================================

    /// <summary>プリセット保存先のディレクトリパス。</summary>
    [JsonIgnore]
    private static string PresetDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ClipboardZenHanConverter",
        "Presets");

    /// <summary>利用可能なプリセット名の一覧を取得します（組込み＋ユーザー保存）。</summary>
    public static string[] GetPresetNames()
    {
        var names = new List<string>(BuiltInPresets.Keys);

        var dir = PresetDirectory;
        if (Directory.Exists(dir))
        {
            names.AddRange(Directory.GetFiles(dir, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => !string.IsNullOrEmpty(name))!);
        }

        return [.. names];
    }

    /// <summary>現在の設定をプリセットとして保存します。</summary>
    /// <param name="name">プリセット名</param>
    public void SavePreset(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        var dir = PresetDirectory;
        Directory.CreateDirectory(dir);

        var filePath = Path.Combine(dir, $"{name}.json");
        SaveToJsonFile(filePath);
    }

    /// <summary>プリセットを読み込んで現在の設定に適用します。</summary>
    /// <param name="name">プリセット名</param>
    /// <returns>読み込みに成功したかどうか</returns>
    public bool LoadPreset(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;

        // 組込みプリセットを優先
        if (BuiltInPresets.TryGetValue(name, out var apply))
        {
            apply(this);
            // 適用後、自動保存をトリガー（PropertyChanged で保存される）
            return true;
        }

        // ファイルベースのプリセット
        var filePath = Path.Combine(PresetDirectory, $"{name}.json");
        return ImportFromFile(filePath);
    }

    /// <summary>プリセットを削除します。</summary>
    /// <param name="name">プリセット名</param>
    public static void DeletePreset(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        if (BuiltInPresets.ContainsKey(name)) return; // 組込みプリセットは削除不可

        var filePath = Path.Combine(PresetDirectory, $"{name}.json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    // ====================================================================
    // プリセット一致検出
    // ====================================================================

    /// <summary>比較対象のプロパティ一覧（ReplacePairs は別途比較）。</summary>
    private static readonly PropertyInfo[] _comparableProperties = typeof(ConvertConfig)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.CanWrite && p.CanRead
            && p.GetCustomAttribute<JsonIgnoreAttribute>() == null
            && p.Name != nameof(ReplacePairs))
        .ToArray();

    /// <summary>組込みプリセットのスナップショットキャッシュです。</summary>
    private static Dictionary<string, Dictionary<string, object?>>? _builtInSnapshotCache;

    /// <summary>2つの ReplacePair リストが等しいか判定します。</summary>
    private static bool ReplacePairsEqual(List<ReplacePair> a, List<ReplacePair> b)
        => a.Count == b.Count && a.Zip(b).All(p => p.First == p.Second);

    /// <summary>指定された2つの ConvertConfig の全比較対象プロパティが等しいかどうかを判定します。</summary>
    private static bool PropertiesEqual(ConvertConfig a, ConvertConfig b)
    {
        foreach (var prop in _comparableProperties)
        {
            var va = prop.GetValue(a);
            var vb = prop.GetValue(b);
            if (!Equals(va, vb)) return false;
        }
        // ReplacePairs は要素ごとに比較
        return ReplacePairsEqual(a.ReplacePairs, b.ReplacePairs);
    }

    /// <summary>組込みプリセットのプロパティスナップショットを取得します。</summary>
    private static Dictionary<string, Dictionary<string, object?>> GetBuiltInSnapshots()
    {
        if (_builtInSnapshotCache is not null) return _builtInSnapshotCache;

        _builtInSnapshotCache = [];
        foreach (var (name, apply) in BuiltInPresets)
        {
            var temp = new ConvertConfig();
            apply(temp);
            var snapshot = new Dictionary<string, object?>(_comparableProperties.Length);
            foreach (var prop in _comparableProperties)
            {
                snapshot[prop.Name] = prop.GetValue(temp);
            }
            _builtInSnapshotCache[name] = snapshot;
        }
        return _builtInSnapshotCache;
    }

    /// <summary>現在の設定に一致するプリセット名を検索します。見つからない場合は null を返します。</summary>
    public string? FindMatchingPreset()
    {
        // 組込みプリセットと比較
        foreach (var (name, snapshot) in GetBuiltInSnapshots())
        {
            var match = true;
            foreach (var prop in _comparableProperties)
            {
                if (!Equals(prop.GetValue(this), snapshot.GetValueOrDefault(prop.Name)))
                {
                    match = false;
                    break;
                }
            }
            if (match) return name;
        }

        // ユーザー保存プリセットと比較
        var dir = PresetDirectory;
        if (!Directory.Exists(dir)) return null;

        var (ctx, type) = SerializeInfo;
        foreach (var filePath in Directory.GetFiles(dir, "*.json"))
        {
            try
            {
                using var stream = File.OpenRead(filePath);
                if (JsonSerializer.Deserialize(stream, type, ctx) is ConvertConfig loaded)
                {
                    if (PropertiesEqual(this, loaded))
                    {
                        return Path.GetFileNameWithoutExtension(filePath);
                    }
                }
            }
            catch
            {
                // 読み込みエラーは無視
            }
        }

        return null;
    }
}


