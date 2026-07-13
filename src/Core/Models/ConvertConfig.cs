using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using ClipboardZenHanConverter.Core.Enums;

namespace ClipboardZenHanConverter.Core.Models;

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

    [ObservableProperty]
    public partial bool IsEnabledZenHan { get; set; }

    [ObservableProperty]
    public partial ZenHanMode ConvertModeNumber { get; set; } = ZenHanMode.None;

    [ObservableProperty]
    public partial ZenHanMode ConvertModeAlphabet { get; set; } = ZenHanMode.None;

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

    [ObservableProperty]
    public partial ZenHanKanaMode ConvertModeKanaHan { get; set; } = ZenHanKanaMode.None;
    [ObservableProperty]
    public partial ZenHanKanaMode ConvertModeKanaZenKata { get; set; } = ZenHanKanaMode.None;
    [ObservableProperty]
    public partial ZenHanKanaMode ConvertModeKanaZenHira { get; set; } = ZenHanKanaMode.None;

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

    [ObservableProperty]
    public partial ZenHanEtcZenHanAsciiMode ConvertModeEtcKanaProlong { get; set; } = ZenHanEtcZenHanAsciiMode.None;
    [ObservableProperty]
    public partial ZenHanEtcZenHanAsciiMode ConvertModeEtcKanaPeriod { get; set; } = ZenHanEtcZenHanAsciiMode.None;
    [ObservableProperty]
    public partial ZenHanEtcZenHanAsciiMode ConvertModeEtcKanaComma { get; set; } = ZenHanEtcZenHanAsciiMode.None;

    [ObservableProperty]
    public partial ZenHanEtcYenMode ConvertModeEtcBSlashHan { get; set; } = ZenHanEtcYenMode.None;
    [ObservableProperty]
    public partial ZenHanEtcYenMode ConvertModeEtcBSlashZen { get; set; } = ZenHanEtcYenMode.None;
    [ObservableProperty]
    public partial ZenHanEtcYenMode ConvertModeEtcYenHan { get; set; } = ZenHanEtcYenMode.None;
    [ObservableProperty]
    public partial ZenHanEtcYenMode ConvertModeEtcYenZen { get; set; } = ZenHanEtcYenMode.None;

    [ObservableProperty]
    public partial ZenHanEtcSpecial ConvertModeEtcTabSpace { get; set; } = ZenHanEtcSpecial.None;
    [ObservableProperty]
    public partial ZenHanEtcSpecial ConvertModeEtcNewline { get; set; } = ZenHanEtcSpecial.None;
    [ObservableProperty]
    public partial ZenHanEtcSpecial ConvertModeEtcMultiSpace { get; set; } = ZenHanEtcSpecial.None;

    public List<ReplacePair> ReplacePairs
    {
        get => _replacePairs;
        set { if (!ReferenceEquals(_replacePairs, value)) { _replacePairs = value; OnPropertyChanged(); } }
    }
    private List<ReplacePair> _replacePairs = [];

    public const string BuiltInPresetAccountingPower = "全力会計（Built-in）";

    private static readonly Dictionary<string, Action<ConvertConfig>> BuiltInPresets = new()
    {
        [BuiltInPresetAccountingPower] = ApplyAccountingPowerPreset,
    };

    private static void ApplyAccountingPowerPreset(ConvertConfig c)
    {
        c.IsEnabledZenHan = true;
        c.ConvertModeNumber = ZenHanMode.ToHan;
        c.ConvertModeAlphabet = ZenHanMode.ToHan;
        c.ConvertModeSymbolParenthesis = ZenHanMode.ToHan;
        c.ConvertModeSymbolHyphenMinus = ZenHanMode.ToHan;
        c.ConvertModeSymbolSlash = ZenHanMode.ToHan;
        c.ConvertModeSymbolComma = ZenHanMode.ToHan;
        c.ConvertModeSymbolPeriod = ZenHanMode.ToHan;
        c.ConvertModeSymbolSpace = ZenHanMode.ToHan;
        // その他の記号 → 全角
        foreach (var prop in typeof(ConvertConfig).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name.StartsWith("ConvertModeSymbol") && p.PropertyType == typeof(ZenHanMode)
                && p.Name is not ("ConvertModeSymbolParenthesis" or "ConvertModeSymbolHyphenMinus"
                    or "ConvertModeSymbolSlash" or "ConvertModeSymbolComma"
                    or "ConvertModeSymbolPeriod" or "ConvertModeSymbolSpace")))
        {
            prop.SetValue(c, ZenHanMode.ToZen);
        }
        c.ConvertModeKanaHan = ZenHanKanaMode.ToZenKata;
        c.ConvertModeKanaZenKata = ZenHanKanaMode.None;
        c.ConvertModeKanaZenHira = ZenHanKanaMode.None;
        c.ConvertModeEtcKanaVoice = ZenHanMode.ToZen;
        c.ConvertModeEtcKanaSemiVoice = ZenHanMode.ToZen;
        c.ConvertModeEtcKanaMiddleDot = ZenHanMode.ToZen;
        c.ConvertModeEtcKanaLeftCornerBracket = ZenHanMode.ToZen;
        c.ConvertModeEtcKanaRightCornerBracket = ZenHanMode.ToZen;
        c.ConvertModeEtcKanaProlong = ZenHanEtcZenHanAsciiMode.ToAscii;
        c.ConvertModeEtcKanaPeriod = ZenHanEtcZenHanAsciiMode.ToHan;
        c.ConvertModeEtcKanaComma = ZenHanEtcZenHanAsciiMode.ToHan;
        c.ConvertModeEtcBSlashHan = ZenHanEtcYenMode.ToHanYen;
        c.ConvertModeEtcBSlashZen = ZenHanEtcYenMode.None;
        c.ConvertModeEtcYenHan = ZenHanEtcYenMode.None;
        c.ConvertModeEtcYenZen = ZenHanEtcYenMode.ToHanYen;
        c.ConvertModeEtcTabSpace = ZenHanEtcSpecial.ToHanSpace;
        c.ConvertModeEtcNewline = ZenHanEtcSpecial.ToHanSpace;
        c.ConvertModeEtcMultiSpace = ZenHanEtcSpecial.ToHanSpace;
    }

    public void ExportToFile(string filePath) => SaveToJsonFile(filePath);

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
        catch { return false; }
    }

    private static string PresetDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ClipboardZenHanConverter", "Presets");

    public static string[] GetPresetNames()
    {
        var names = new List<string>(BuiltInPresets.Keys);
        var dir = PresetDirectory;
        if (Directory.Exists(dir))
            names.AddRange(Directory.GetFiles(dir, "*.json").Select(Path.GetFileNameWithoutExtension).Where(n => !string.IsNullOrEmpty(n))!);
        return [.. names];
    }

    public void SavePreset(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        Directory.CreateDirectory(PresetDirectory);
        SaveToJsonFile(Path.Combine(PresetDirectory, $"{name}.json"));
    }

    public bool LoadPreset(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        if (BuiltInPresets.TryGetValue(name, out var apply)) { apply(this); return true; }
        return ImportFromFile(Path.Combine(PresetDirectory, $"{name}.json"));
    }

    public static void DeletePreset(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || BuiltInPresets.ContainsKey(name)) return;
        var fp = Path.Combine(PresetDirectory, $"{name}.json");
        if (File.Exists(fp)) File.Delete(fp);
    }

    private static readonly PropertyInfo[] _comparableProperties = typeof(ConvertConfig)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.CanWrite && p.CanRead && p.GetCustomAttribute<JsonIgnoreAttribute>() == null && p.Name != nameof(ReplacePairs))
        .ToArray();

    private static Dictionary<string, Dictionary<string, object?>>? _builtInSnapshotCache;

    private static bool ReplacePairsEqual(List<ReplacePair> a, List<ReplacePair> b)
        => a.Count == b.Count && a.Zip(b).All(p => p.First == p.Second);

    private static bool PropertiesEqual(ConvertConfig a, ConvertConfig b)
    {
        foreach (var prop in _comparableProperties)
        {
            if (!Equals(prop.GetValue(a), prop.GetValue(b))) return false;
        }
        return ReplacePairsEqual(a.ReplacePairs, b.ReplacePairs);
    }

    private static Dictionary<string, Dictionary<string, object?>> GetBuiltInSnapshots()
    {
        if (_builtInSnapshotCache is not null) return _builtInSnapshotCache;
        _builtInSnapshotCache = [];
        foreach (var (name, apply) in BuiltInPresets)
        {
            var temp = new ConvertConfig();
            apply(temp);
            var snapshot = new Dictionary<string, object?>(_comparableProperties.Length);
            foreach (var prop in _comparableProperties) snapshot[prop.Name] = prop.GetValue(temp);
            _builtInSnapshotCache[name] = snapshot;
        }
        return _builtInSnapshotCache;
    }

    public string? FindMatchingPreset()
    {
        foreach (var (name, snapshot) in GetBuiltInSnapshots())
        {
            if (_comparableProperties.All(prop => Equals(prop.GetValue(this), snapshot.GetValueOrDefault(prop.Name))))
                return name;
        }
        var dir = PresetDirectory;
        if (!Directory.Exists(dir)) return null;
        var (ctx, type) = SerializeInfo;
        foreach (var fp in Directory.GetFiles(dir, "*.json"))
        {
            try
            {
                using var stream = File.OpenRead(fp);
                if (JsonSerializer.Deserialize(stream, type, ctx) is ConvertConfig loaded && PropertiesEqual(this, loaded))
                    return Path.GetFileNameWithoutExtension(fp);
            }
            catch { }
        }
        return null;
    }
}
