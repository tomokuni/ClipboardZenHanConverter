using ClipboardZenHanConverter.Core.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Frozen;
using System.Text.Json;

namespace ClipboardZenHanConverter.Core.Models;

/// <summary>モードプロパティの単一定義。</summary>
/// <remarks>Get/Set デリゲートのペアで、ConvertConfig のモードプロパティを型安全にアクセスします。<br/>
/// ToZenValue に値が設定されている場合、そのプロパティは「全力会計」プリセットで全角設定の対象となります。<br/>
/// UI 側はこの定義を直接参照することで、プロパティ名文字列による実行時名前解決を避けます（AOT 互換）。</remarks>
public sealed record ModePropDef(
    Func<ConvertConfig, object> Get,
    Action<ConvertConfig, object> Set,
    object? ToZenValue);           // null → 全角設定対象外

/// <summary>全角/半角変換の設定を管理します。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - 全角/半角変換の有効/無効と、数字・英字・記号・かな・約物など45以上の個別変換モードプロパティ<br/>
/// - ユーザー定義の置換ルール（正規表現対応）<br/>
/// - プリセット保存/読み込み/削除（Built-in + ユーザー定義）<br/>
/// - インポート/エクスポート（JSONファイル）<br/>
/// - 自動永続化（SettingsPersistenceBase によるデバウンス保存）<br/><br/>
/// 特徴: <br/>
/// - _modeProps 配列によるメタデータ駆動で、コピー・比較・全角設定のコード重複を排除（DRY）<br/>
/// - FrozenDictionary による組み込みプリセットの高速ルックアップ<br/>
/// - CommunityToolkit.Mvvm の ObservableProperty 生成による変更通知<br/>
/// - System.Text.Json ソースジェネレーター対応（AppJsonContext）<br/><br/>
/// 最適化手法: <br/>
/// - _modeProps から動生成された _copyActions/_compareActions/_toZenSymbolSetters 配列<br/>
/// - ループによる一括適用で個別プロパティの列挙コストを削減<br/>
/// - プリセット比較は PropertiesEqual で全プロパティ一致を検証</remarks>
public partial class ConvertConfig : SettingsPersistenceBase<ConvertConfig>
{
    /// <summary>シリアライズに使用する JsonSerializerContext と型を取得します。</summary>
    protected override SerializableTypeInfo SerializeInfo
        => new(AppJsonContext.Default, typeof(ConvertConfig));

    /// <summary>ConvertConfig の新しいインスタンスを初期化します。自動保存先を %LOCALAPPDATA% 配下に設定します。</summary>
    public ConvertConfig()
    {
        AutoSaveFileName = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ClipboardZenHanConverter",
            "Settings.json");
    }

    // ─── 全モードプロパティの単一定義（これ1つでコピー・比較・全角設定を生成） ───

    /// <summary>全モードプロパティの型安全なアクセサ定義（Canonical Source）。</summary>
    /// <remarks>ConvertConfig の各モードプロパティへアクセスする Get/Set デリゲートを一元管理します。<br/>
    /// SegmentDefinitions はこの定義を直接参照することで、プロパティ名文字列による実行時名前解決を避けます（NativeAOT 互換）。<br/>
    /// All はコピー・比較・全角設定の一括適用に使用する全定義の集合です。</remarks>
    public static class Mode
    {
        /// <summary>全角/半角変換の有効/無効の定義。</summary>
        public static readonly ModePropDef IsEnabledZenHan = new(c => c.IsEnabledZenHan, (c, v) => c.IsEnabledZenHan = (bool)v, null);

        // 数値・英字は会計帳票でも半角が必須のため、全角設定の対象外（ToZenValue = null）とします。
        public static readonly ModePropDef Number = new(c => c.ConvertModeNumber, (c, v) => c.ConvertModeNumber = (ZenHanMode)v, null);
        public static readonly ModePropDef Alphabet = new(c => c.ConvertModeAlphabet, (c, v) => c.ConvertModeAlphabet = (ZenHanMode)v, null);

        public static readonly ModePropDef SymbolParenthesis = new(c => c.ConvertModeSymbolParenthesis, (c, v) => c.ConvertModeSymbolParenthesis = (ZenHanMode)v, null);
        public static readonly ModePropDef SymbolSquareBracket = new(c => c.ConvertModeSymbolSquareBracket, (c, v) => c.ConvertModeSymbolSquareBracket = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolCurlyBracket = new(c => c.ConvertModeSymbolCurlyBracket, (c, v) => c.ConvertModeSymbolCurlyBracket = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolDoubleQuote = new(c => c.ConvertModeSymbolDoubleQuote, (c, v) => c.ConvertModeSymbolDoubleQuote = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolSingleQuote = new(c => c.ConvertModeSymbolSingleQuote, (c, v) => c.ConvertModeSymbolSingleQuote = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolComma = new(c => c.ConvertModeSymbolComma, (c, v) => c.ConvertModeSymbolComma = (ZenHanMode)v, null);
        public static readonly ModePropDef SymbolPeriod = new(c => c.ConvertModeSymbolPeriod, (c, v) => c.ConvertModeSymbolPeriod = (ZenHanMode)v, null);
        public static readonly ModePropDef SymbolColon = new(c => c.ConvertModeSymbolColon, (c, v) => c.ConvertModeSymbolColon = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolSemicolon = new(c => c.ConvertModeSymbolSemicolon, (c, v) => c.ConvertModeSymbolSemicolon = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolLessThan = new(c => c.ConvertModeSymbolLessThan, (c, v) => c.ConvertModeSymbolLessThan = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolEqual = new(c => c.ConvertModeSymbolEqual, (c, v) => c.ConvertModeSymbolEqual = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolGreaterThan = new(c => c.ConvertModeSymbolGreaterThan, (c, v) => c.ConvertModeSymbolGreaterThan = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolPlus = new(c => c.ConvertModeSymbolPlus, (c, v) => c.ConvertModeSymbolPlus = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolHyphenMinus = new(c => c.ConvertModeSymbolHyphenMinus, (c, v) => c.ConvertModeSymbolHyphenMinus = (ZenHanMode)v, null);
        public static readonly ModePropDef SymbolExclamation = new(c => c.ConvertModeSymbolExclamation, (c, v) => c.ConvertModeSymbolExclamation = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolSharp = new(c => c.ConvertModeSymbolSharp, (c, v) => c.ConvertModeSymbolSharp = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolDollar = new(c => c.ConvertModeSymbolDollar, (c, v) => c.ConvertModeSymbolDollar = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolPercent = new(c => c.ConvertModeSymbolPercent, (c, v) => c.ConvertModeSymbolPercent = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolAmpersand = new(c => c.ConvertModeSymbolAmpersand, (c, v) => c.ConvertModeSymbolAmpersand = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolAsterisk = new(c => c.ConvertModeSymbolAsterisk, (c, v) => c.ConvertModeSymbolAsterisk = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolSlash = new(c => c.ConvertModeSymbolSlash, (c, v) => c.ConvertModeSymbolSlash = (ZenHanMode)v, null);
        public static readonly ModePropDef SymbolQuestion = new(c => c.ConvertModeSymbolQuestion, (c, v) => c.ConvertModeSymbolQuestion = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolAt = new(c => c.ConvertModeSymbolAt, (c, v) => c.ConvertModeSymbolAt = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolCaret = new(c => c.ConvertModeSymbolCaret, (c, v) => c.ConvertModeSymbolCaret = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolUnderBar = new(c => c.ConvertModeSymbolUnderBar, (c, v) => c.ConvertModeSymbolUnderBar = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolBackquote = new(c => c.ConvertModeSymbolBackquote, (c, v) => c.ConvertModeSymbolBackquote = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolVerticalBar = new(c => c.ConvertModeSymbolVerticalBar, (c, v) => c.ConvertModeSymbolVerticalBar = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolTilde = new(c => c.ConvertModeSymbolTilde, (c, v) => c.ConvertModeSymbolTilde = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef SymbolSpace = new(c => c.ConvertModeSymbolSpace, (c, v) => c.ConvertModeSymbolSpace = (ZenHanMode)v, null);

        public static readonly ModePropDef KanaHan = new(c => c.ConvertModeKanaHan, (c, v) => c.ConvertModeKanaHan = (ZenHanKanaMode)v, null);
        public static readonly ModePropDef KanaZenKata = new(c => c.ConvertModeKanaZenKata, (c, v) => c.ConvertModeKanaZenKata = (ZenHanKanaMode)v, null);
        public static readonly ModePropDef KanaZenHira = new(c => c.ConvertModeKanaZenHira, (c, v) => c.ConvertModeKanaZenHira = (ZenHanKanaMode)v, null);

        public static readonly ModePropDef EtcKanaVoice = new(c => c.ConvertModeEtcKanaVoice, (c, v) => c.ConvertModeEtcKanaVoice = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef EtcKanaSemiVoice = new(c => c.ConvertModeEtcKanaSemiVoice, (c, v) => c.ConvertModeEtcKanaSemiVoice = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef EtcKanaMiddleDot = new(c => c.ConvertModeEtcKanaMiddleDot, (c, v) => c.ConvertModeEtcKanaMiddleDot = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef EtcKanaLeftCornerBracket = new(c => c.ConvertModeEtcKanaLeftCornerBracket, (c, v) => c.ConvertModeEtcKanaLeftCornerBracket = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef EtcKanaRightCornerBracket = new(c => c.ConvertModeEtcKanaRightCornerBracket, (c, v) => c.ConvertModeEtcKanaRightCornerBracket = (ZenHanMode)v, ZenHanMode.ToZen);
        public static readonly ModePropDef EtcKanaProlong = new(c => c.ConvertModeEtcKanaProlong, (c, v) => c.ConvertModeEtcKanaProlong = (ZenHanEtcZenHanAsciiMode)v, null);
        public static readonly ModePropDef EtcKanaPeriod = new(c => c.ConvertModeEtcKanaPeriod, (c, v) => c.ConvertModeEtcKanaPeriod = (ZenHanEtcZenHanAsciiMode)v, null);
        public static readonly ModePropDef EtcKanaComma = new(c => c.ConvertModeEtcKanaComma, (c, v) => c.ConvertModeEtcKanaComma = (ZenHanEtcZenHanAsciiMode)v, null);

        public static readonly ModePropDef EtcBSlashHan = new(c => c.ConvertModeEtcBSlashHan, (c, v) => c.ConvertModeEtcBSlashHan = (ZenHanEtcYenMode)v, null);
        public static readonly ModePropDef EtcBSlashZen = new(c => c.ConvertModeEtcBSlashZen, (c, v) => c.ConvertModeEtcBSlashZen = (ZenHanEtcYenMode)v, null);
        public static readonly ModePropDef EtcYenHan = new(c => c.ConvertModeEtcYenHan, (c, v) => c.ConvertModeEtcYenHan = (ZenHanEtcYenMode)v, null);
        public static readonly ModePropDef EtcYenZen = new(c => c.ConvertModeEtcYenZen, (c, v) => c.ConvertModeEtcYenZen = (ZenHanEtcYenMode)v, null);

        public static readonly ModePropDef EtcTabSpace = new(c => c.ConvertModeEtcTabSpace, (c, v) => c.ConvertModeEtcTabSpace = (ZenHanEtcSpecial)v, null);
        public static readonly ModePropDef EtcNewline = new(c => c.ConvertModeEtcNewline, (c, v) => c.ConvertModeEtcNewline = (ZenHanEtcSpecial)v, null);
        public static readonly ModePropDef EtcMultiSpace = new(c => c.ConvertModeEtcMultiSpace, (c, v) => c.ConvertModeEtcMultiSpace = (ZenHanEtcSpecial)v, null);

        /// <summary>全モードプロパティの定義集合。コピー・比較・全角設定の一括適用に使用します。</summary>
        internal static readonly ModePropDef[] All =
        [
            IsEnabledZenHan,
            Number, Alphabet,
            SymbolParenthesis, SymbolSquareBracket, SymbolCurlyBracket, SymbolDoubleQuote, SymbolSingleQuote,
            SymbolComma, SymbolPeriod, SymbolColon, SymbolSemicolon, SymbolLessThan, SymbolEqual, SymbolGreaterThan,
            SymbolPlus, SymbolHyphenMinus, SymbolExclamation, SymbolSharp, SymbolDollar, SymbolPercent,
            SymbolAmpersand, SymbolAsterisk, SymbolSlash, SymbolQuestion, SymbolAt, SymbolCaret, SymbolUnderBar,
            SymbolBackquote, SymbolVerticalBar, SymbolTilde, SymbolSpace,
            KanaHan, KanaZenKata, KanaZenHira,
            EtcKanaVoice, EtcKanaSemiVoice, EtcKanaMiddleDot, EtcKanaLeftCornerBracket, EtcKanaRightCornerBracket,
            EtcKanaProlong, EtcKanaPeriod, EtcKanaComma,
            EtcBSlashHan, EtcBSlashZen, EtcYenHan, EtcYenZen,
            EtcTabSpace, EtcNewline, EtcMultiSpace,
        ];
    }

    /// <summary>全モードプロパティのメタデータ定義配列。コピー・比較・全角設定の一括適用に使用します。</summary>
    static readonly ModePropDef[] _modeProps = Mode.All;

    /// <summary>_modeProps から生成されたコピーアクション。各要素は (src, dst) => dst.Prop = src.Prop を実行します。</summary>
    private static readonly Action<ConvertConfig, ConvertConfig>[] _copyActions =
        [.. _modeProps.Select(m => (Action<ConvertConfig, ConvertConfig>)((s, t) => m.Set(t, m.Get(s))))];

    /// <summary>_modeProps から生成された比較アクション。各要素は (a, b) => a.Prop == b.Prop を実行します。</summary>
    private static readonly Func<ConvertConfig, ConvertConfig, bool>[] _compareActions =
        [.. _modeProps.Select(m => (Func<ConvertConfig, ConvertConfig, bool>)((a, b) => Equals(m.Get(a), m.Get(b))))];

    /// <summary>_modeProps から生成された全角設定セッター。各要素は c => c.Prop = ToZenValue を実行します。</summary>
    private static readonly Action<ConvertConfig>[] _toZenSymbolSetters =
        [.. _modeProps.Where(m => m.ToZenValue is not null)
            .Select(m => (Action<ConvertConfig>)(c => m.Set(c, m.ToZenValue!)))];

    /// <summary>他の ConvertConfig インスタンスから全設定をコピーします。</summary>
    /// <param name="other">コピー元の設定インスタンス。</param>
    /// <remarks>_copyActions 配列を使用して全モードプロパティを一括コピーし、置換ルールも複製します。</remarks>
    protected override void ApplyFrom(ConvertConfig other)
    {
        foreach (var action in _copyActions) action(other, this);
        ReplacePairs = [.. other.ReplacePairs];
    }

    /// <summary>全角/半角変換の有効/無効を取得または設定します。</summary>
    /// <remarks>変換を実行するか否かは呼び出し側（画面側）のポリシーであり、変換エンジンはこの値を参照しません。<br/>
    /// 画面側がこの値で変換の実行可否を切り替えます。</remarks>
    [ObservableProperty]
    public partial bool IsEnabledZenHan { get; set; }

    /// <summary>数字（0-9）の変換モードを取得または設定します。</summary>
    /// <remarks>全角数字「１２３」と半角数字「123」の相互変換方向を指定します。CharConverter の SymbolMap で参照されます。</remarks>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeNumber { get; set; } = ZenHanMode.None;

    /// <summary>英字（A-Z, a-z）の変換モードを取得または設定します。</summary>
    /// <remarks>全角英字「ＡＢＣ」「ａｂｃ」と半角英字「ABC」「abc」の相互変換方向を指定します。</remarks>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeAlphabet { get; set; } = ZenHanMode.None;

    /// <summary>丸括弧（）の変換モードを取得または設定します。</summary>
    /// <remarks>全角括弧「（）」と半角括弧「()」の相互変換方向を指定します。CharConverter では IsPair=true として左右同時に処理されます。</remarks>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolParenthesis { get; set; } = ZenHanMode.None;

    /// <summary>角括弧［］の変換モードを取得または設定します。</summary>
    /// <remarks>全角角括弧「［］」と半角角括弧「[]」の相互変換方向を指定します。</remarks>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolSquareBracket { get; set; } = ZenHanMode.None;

    /// <summary>波括弧｛｝の変換モードを取得または設定します。</summary>
    /// <remarks>全角波括弧「｛｝」と半角波括弧「{}」の相互変換方向を指定します。</remarks>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolCurlyBracket { get; set; } = ZenHanMode.None;

    /// <summary>ダブルクォート"の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolDoubleQuote { get; set; } = ZenHanMode.None;

    /// <summary>シングルクォート'の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolSingleQuote { get; set; } = ZenHanMode.None;

    /// <summary>カンマ，の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolComma { get; set; } = ZenHanMode.None;

    /// <summary>ピリオド．の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolPeriod { get; set; } = ZenHanMode.None;

    /// <summary>コロン：の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolColon { get; set; } = ZenHanMode.None;

    /// <summary>セミコロン；の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolSemicolon { get; set; } = ZenHanMode.None;

    /// <summary>不等号＜の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolLessThan { get; set; } = ZenHanMode.None;

    /// <summary>イコール＝の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolEqual { get; set; } = ZenHanMode.None;

    /// <summary>不等号＞の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolGreaterThan { get; set; } = ZenHanMode.None;

    /// <summary>プラス＋の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolPlus { get; set; } = ZenHanMode.None;

    /// <summary>マイナス－の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolHyphenMinus { get; set; } = ZenHanMode.None;

    /// <summary>感嘆符！の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolExclamation { get; set; } = ZenHanMode.None;

    /// <summary>シャープ＃の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolSharp { get; set; } = ZenHanMode.None;

    /// <summary>ダラー＄の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolDollar { get; set; } = ZenHanMode.None;

    /// <summary>パーセント％の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolPercent { get; set; } = ZenHanMode.None;

    /// <summary>アンパサンド＆の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolAmpersand { get; set; } = ZenHanMode.None;

    /// <summary>アスタリスク＊の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolAsterisk { get; set; } = ZenHanMode.None;

    /// <summary>スラッシュ／の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolSlash { get; set; } = ZenHanMode.None;

    /// <summary>疑問符？の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolQuestion { get; set; } = ZenHanMode.None;

    /// <summary>アットマーク＠の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolAt { get; set; } = ZenHanMode.None;

    /// <summary>キャレット＾の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolCaret { get; set; } = ZenHanMode.None;

    /// <summary>アンダースコア＿の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolUnderBar { get; set; } = ZenHanMode.None;

    /// <summary>バッククォート`の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolBackquote { get; set; } = ZenHanMode.None;

    /// <summary>縦棒｜の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolVerticalBar { get; set; } = ZenHanMode.None;

    /// <summary>チルダ～の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolTilde { get; set; } = ZenHanMode.None;

    /// <summary>スペース␣の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeSymbolSpace { get; set; } = ZenHanMode.None;

    /// <summary>半角カナの変換モードを取得または設定します。</summary>
    /// <remarks>半角カナ「ｱｲｳｴｵ」の変換方向を指定します。全角カタカナまたは全角ひらがなへの変換が選択可能です。</remarks>
    [ObservableProperty]
    public partial ZenHanKanaMode ConvertModeKanaHan { get; set; } = ZenHanKanaMode.None;

    /// <summary>全角カタカナの変換モードを取得または設定します。</summary>
    /// <remarks>全角カタカナ「アイウエオ」の変換方向を指定します。半角カナまたは全角ひらがなへの変換が選択可能です。</remarks>
    [ObservableProperty]
    public partial ZenHanKanaMode ConvertModeKanaZenKata { get; set; } = ZenHanKanaMode.None;

    /// <summary>全角ひらがなの変換モードを取得または設定します。</summary>
    /// <remarks>全角ひらがな「あいうえお」の変換方向を指定します。半角カナまたは全角カタカナへの変換が選択可能です。</remarks>
    [ObservableProperty]
    public partial ZenHanKanaMode ConvertModeKanaZenHira { get; set; } = ZenHanKanaMode.None;

    /// <summary>かな 濁点゛の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeEtcKanaVoice { get; set; } = ZenHanMode.None;

    /// <summary>かな 半濁点゜の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeEtcKanaSemiVoice { get; set; } = ZenHanMode.None;

    /// <summary>かな 中点・の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeEtcKanaMiddleDot { get; set; } = ZenHanMode.None;

    /// <summary>かな 左上括弧「の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeEtcKanaLeftCornerBracket { get; set; } = ZenHanMode.None;

    /// <summary>かな 右下括弧」の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanMode ConvertModeEtcKanaRightCornerBracket { get; set; } = ZenHanMode.None;

    /// <summary>かな 長音記号ーの変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanEtcZenHanAsciiMode ConvertModeEtcKanaProlong { get; set; } = ZenHanEtcZenHanAsciiMode.None;

    /// <summary>かな 読点。の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanEtcZenHanAsciiMode ConvertModeEtcKanaPeriod { get; set; } = ZenHanEtcZenHanAsciiMode.None;

    /// <summary>かな 句点、の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanEtcZenHanAsciiMode ConvertModeEtcKanaComma { get; set; } = ZenHanEtcZenHanAsciiMode.None;

    /// <summary>バックスラッシュ半角\の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanEtcYenMode ConvertModeEtcBSlashHan { get; set; } = ZenHanEtcYenMode.None;

    /// <summary>バックスラッシュ全角＼の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanEtcYenMode ConvertModeEtcBSlashZen { get; set; } = ZenHanEtcYenMode.None;

    /// <summary>円記号半角¥の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanEtcYenMode ConvertModeEtcYenHan { get; set; } = ZenHanEtcYenMode.None;

    /// <summary>円記号全角￥の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanEtcYenMode ConvertModeEtcYenZen { get; set; } = ZenHanEtcYenMode.None;

    /// <summary>タブ文字の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanEtcSpecial ConvertModeEtcTabSpace { get; set; } = ZenHanEtcSpecial.None;

    /// <summary>改行文字の変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanEtcSpecial ConvertModeEtcNewline { get; set; } = ZenHanEtcSpecial.None;

    /// <summary>連続スペースの変換モードを取得または設定します。</summary>
    [ObservableProperty]
    public partial ZenHanEtcSpecial ConvertModeEtcMultiSpace { get; set; } = ZenHanEtcSpecial.None;

    /// <summary>ユーザー定義の置換ルール一覧。</summary>
    /// <remarks>設定画面の DataGrid で編集され、CharConverter.ApplyUserReplacements で変換時に適用されます。</remarks>
    [ObservableProperty]
    public partial List<ReplacePair> ReplacePairs { get; set; } = [];

    /// <summary>組み込みプリセット「全力会計」の定数名。</summary>
    public const string BuiltInPresetAccountingPower = "全力会計（Built-in）";

    /// <summary>組み込みプリセット「英数記号半角、かな全角」の定数名。</summary>
    public const string BuiltInPresetAlphanumericHanKanaZen = "英数記号半角、かな全角（Built-in）";

    /// <summary>指定されたプリセット名が組み込みプリセットかどうかを判定します。</summary>
    /// <param name="name">プリセット名</param>
    /// <returns>組み込みプリセットの場合は true</returns>
    public static bool IsBuiltInPreset(string name) => BuiltInPresets.ContainsKey(name);

    /// <summary>組み込みプリセット定義。FrozenDictionary による高速・不変なルックアップ。</summary>
    private static readonly FrozenDictionary<string, Action<ConvertConfig>> BuiltInPresets =
        new Dictionary<string, Action<ConvertConfig>>
        {
            [BuiltInPresetAccountingPower] = ApplyAccountingPowerPreset,
            [BuiltInPresetAlphanumericHanKanaZen] = ApplyAlphanumericHanKanaZenPreset,
        }.ToFrozenDictionary();

    /// <summary>組み込みプリセット「英数記号半角、かな全角」を適用します。</summary>
    /// <param name="c">設定を適用する ConvertConfig インスタンス。</param>
    /// <remarks>英字/数字/記号を半角、半角カナを全角カタカナ、バックスラッシュ/円記号を半角円記号に統一します。<br/>
    /// 改行は変換しません。</remarks>
    private static void ApplyAlphanumericHanKanaZenPreset(ConvertConfig c)
    {
        c.IsEnabledZenHan = true;
        c.ConvertModeNumber = ZenHanMode.ToHan;
        c.ConvertModeAlphabet = ZenHanMode.ToHan;
        // 英数記号 → 全て半角
        c.ConvertModeSymbolParenthesis = ZenHanMode.ToHan;
        c.ConvertModeSymbolSquareBracket = ZenHanMode.ToHan;
        c.ConvertModeSymbolCurlyBracket = ZenHanMode.ToHan;
        c.ConvertModeSymbolDoubleQuote = ZenHanMode.ToHan;
        c.ConvertModeSymbolSingleQuote = ZenHanMode.ToHan;
        c.ConvertModeSymbolComma = ZenHanMode.ToHan;
        c.ConvertModeSymbolPeriod = ZenHanMode.ToHan;
        c.ConvertModeSymbolColon = ZenHanMode.ToHan;
        c.ConvertModeSymbolSemicolon = ZenHanMode.ToHan;
        c.ConvertModeSymbolLessThan = ZenHanMode.ToHan;
        c.ConvertModeSymbolEqual = ZenHanMode.ToHan;
        c.ConvertModeSymbolGreaterThan = ZenHanMode.ToHan;
        c.ConvertModeSymbolPlus = ZenHanMode.ToHan;
        c.ConvertModeSymbolHyphenMinus = ZenHanMode.ToHan;
        c.ConvertModeSymbolExclamation = ZenHanMode.ToHan;
        c.ConvertModeSymbolSharp = ZenHanMode.ToHan;
        c.ConvertModeSymbolDollar = ZenHanMode.ToHan;
        c.ConvertModeSymbolPercent = ZenHanMode.ToHan;
        c.ConvertModeSymbolAmpersand = ZenHanMode.ToHan;
        c.ConvertModeSymbolAsterisk = ZenHanMode.ToHan;
        c.ConvertModeSymbolSlash = ZenHanMode.ToHan;
        c.ConvertModeSymbolQuestion = ZenHanMode.ToHan;
        c.ConvertModeSymbolAt = ZenHanMode.ToHan;
        c.ConvertModeSymbolCaret = ZenHanMode.ToHan;
        c.ConvertModeSymbolUnderBar = ZenHanMode.ToHan;
        c.ConvertModeSymbolBackquote = ZenHanMode.ToHan;
        c.ConvertModeSymbolVerticalBar = ZenHanMode.ToHan;
        c.ConvertModeSymbolTilde = ZenHanMode.ToHan;
        c.ConvertModeSymbolSpace = ZenHanMode.ToHan;
        // かな → 半角カナ→全角カタカナ、全角カタカナ/ひらがなは変換なし
        c.ConvertModeKanaHan = ZenHanKanaMode.ToZenKata;
        c.ConvertModeKanaZenKata = ZenHanKanaMode.None;
        c.ConvertModeKanaZenHira = ZenHanKanaMode.None;
        // かな記号 → 半角
        c.ConvertModeEtcKanaVoice = ZenHanMode.ToHan;
        c.ConvertModeEtcKanaSemiVoice = ZenHanMode.ToHan;
        c.ConvertModeEtcKanaMiddleDot = ZenHanMode.ToHan;
        c.ConvertModeEtcKanaLeftCornerBracket = ZenHanMode.ToHan;
        c.ConvertModeEtcKanaRightCornerBracket = ZenHanMode.ToHan;
        // かな約物 → 全角
        c.ConvertModeEtcKanaProlong = ZenHanEtcZenHanAsciiMode.ToZen;
        c.ConvertModeEtcKanaPeriod = ZenHanEtcZenHanAsciiMode.ToZen;
        c.ConvertModeEtcKanaComma = ZenHanEtcZenHanAsciiMode.ToZen;
        // バックスラッシュ/円記号 → 半角円記号
        c.ConvertModeEtcBSlashHan = ZenHanEtcYenMode.ToHanYen;
        c.ConvertModeEtcBSlashZen = ZenHanEtcYenMode.ToHanYen;
        c.ConvertModeEtcYenHan = ZenHanEtcYenMode.None;
        c.ConvertModeEtcYenZen = ZenHanEtcYenMode.ToHanYen;
        // 特殊文字: タブを半角スペース、改行は変換なし、連続スペースを統合
        c.ConvertModeEtcTabSpace = ZenHanEtcSpecial.ToHanSpace;
        c.ConvertModeEtcNewline = ZenHanEtcSpecial.None;
        c.ConvertModeEtcMultiSpace = ZenHanEtcSpecial.ToHanSpace;
        c.ReplacePairs = [];
    }

    /// <summary>組み込みプリセット「全力会計」を適用します。
    /// <param name="c">設定を適用する ConvertConfig インスタンス。</param>
    /// <remarks>数字/英字/一部記号を半角、その他記号/かなを全角にする会計帳票向け設定です。</remarks>
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
        // その他の記号 → 全角（型安全な Action 配列）
        foreach (var setter in _toZenSymbolSetters) setter(c);
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
        c.ReplacePairs = [];
    }

    /// <summary>現在の設定を JSON ファイルにエクスポートします。</summary>
    /// <param name="filePath">エクスポート先のファイルパス</param>
    public void ExportToFile(string filePath) => SaveToJsonFile(filePath);

    /// <summary>JSON ファイルから設定をインポートします。</summary>
    /// <param name="filePath">インポート元のファイルパス</param>
    /// <returns>インポートに成功した場合は true。ファイルが存在しない、または JSON が不正な場合は false。</returns>
    /// <remarks>インポート中は自動保存を一時的に無効化し、インポート完了後に自動保存ファイルも更新します。</remarks>
    public bool ImportFromFile(string filePath)
    {
        if (!File.Exists(filePath)) return false;
        try
        {
            var info = SerializeInfo;
            using var stream = File.OpenRead(filePath);
            if (JsonSerializer.Deserialize(stream, info.Type, info.Context) is not ConvertConfig loaded) return false;
            var wasAutoSave = IsAutoSave;
            IsAutoSave = false;
            ApplyFrom(loaded);
            IsAutoSave = wasAutoSave;
            SaveToJsonFile(AutoSaveFileName);
            return true;
        }
        catch (JsonException)
        {
            // 不正なJSONファイルによるデシリアライズ失敗。呼び出し元が false を「失敗」として扱う。
            return false;
        }
        catch (IOException)
        {
            // ファイル読み取り権限不足などI/Oエラーも false で報告。
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            // アクセス権限不足による読み取り失敗も false で報告。
            return false;
        }
    }

    /// <summary>ユーザープリセットの保存ディレクトリパスを取得または設定します。</summary>
    /// <value>既定は %LOCALAPPDATA%\ClipboardZenHanConverter\Presets。</value>
    /// <remarks>テストでは一時ディレクトリへ差し替えて、実ユーザーのプリセットを汚さないようにします。</remarks>
    public static string PresetDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ClipboardZenHanConverter", "Presets");

    /// <summary>利用可能なプリセット名の一覧を取得します。</summary>
    /// <returns>プリセット名の配列。組み込みプリセット + ユーザー定義プリセット（.json）</returns>
    /// <remarks>並び順は「組込み→保存」の優先順で、各グループ内では名前の昇順です。</remarks>
    public static string[] GetPresetNames()
    {
        var names = new List<string>();
        names.AddRange(BuiltInPresets.Keys.OrderBy(n => n, StringComparer.Ordinal));
        var dir = PresetDirectory;
        if (Directory.Exists(dir))
            names.AddRange(
                Directory.GetFiles(dir, "*.json")
                    .Select(Path.GetFileNameWithoutExtension)
                    .OfType<string>()
                    .OrderBy(n => n, StringComparer.Ordinal));
        return [.. names];
    }

    /// <summary>現在の設定をプリセットとして保存します。</summary>
    /// <param name="name">プリセット名。空または空白の場合は何もしない。</param>
    /// <remarks>保存先: %LOCALAPPDATA%\ClipboardZenHanConverter\Presets\{name}.json</remarks>
    public void SavePreset(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        Directory.CreateDirectory(PresetDirectory);
        SaveToJsonFile(Path.Combine(PresetDirectory, $"{name}.json"));
    }

    /// <summary>指定されたプリセットを読み込みます。</summary>
    /// <param name="name">プリセット名。組み込みプリセット名の場合は BuiltInPresets から適用。</param>
    /// <returns>読み込みに成功した場合は true</returns>
    /// <remarks>組み込みプリセットは BuiltInPresets Dictionary から即時適用されます。<br/>
    /// ユーザープリセットは Presets ディレクトリの JSON ファイルからインポートされます。</remarks>
    public bool LoadPreset(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        if (BuiltInPresets.TryGetValue(name, out var apply)) { apply(this); return true; }
        return ImportFromFile(Path.Combine(PresetDirectory, $"{name}.json"));
    }

    /// <summary>指定されたユーザープリセットを削除します。</summary>
    /// <param name="name">削除するプリセット名。組み込みプリセットは削除されません。</param>
    /// <remarks>組み込みプリセット（BuiltInPresets）の削除はできません。<br/>
    /// ファイルが存在しない場合は何もしません。</remarks>
    public static void DeletePreset(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || BuiltInPresets.ContainsKey(name)) return;
        var fp = Path.Combine(PresetDirectory, $"{name}.json");
        if (File.Exists(fp)) File.Delete(fp);
    }

    /// <summary>2つの ConvertConfig の全プロパティが等しいかを検証します。</summary>
    /// <param name="a">比較対象A</param>
    /// <param name="b">比較対象B</param>
    /// <returns>全プロパティが一致する場合は true</returns>
    /// <remarks>_compareActions 配列を使用して全モードプロパティを比較します。置換ルールも件数と内容を比較します。</remarks>
    private static bool PropertiesEqual(ConvertConfig a, ConvertConfig b)
    {
        foreach (var cmp in _compareActions)
            if (!cmp(a, b)) return false;
        return a.ReplacePairs.Count == b.ReplacePairs.Count && a.ReplacePairs.Zip(b.ReplacePairs).All(p => p.First == p.Second);
    }

    /// <summary>現在の設定と一致するプリセットを検索します。</summary>
    /// <returns>一致するプリセット名。見つからない場合は null。</returns>
    /// <remarks>GetPresetNames() と同じ並び順（組込み→保存、各グループ内で名前昇順）で先頭から順に一致確認します。<br/>
    /// 比較は _compareActions 配列を使用した全モードプロパティの一致検証で行われます。</remarks>
    public string? FindMatchingPreset()
    {
        // GetPresetNames() と同じ並び順で一致確認
        foreach (var name in GetPresetNames())
        {
            if (BuiltInPresets.TryGetValue(name, out var apply))
            {
                var temp = new ConvertConfig();
                apply(temp);
                if (PropertiesEqual(this, temp)) return name;
            }
            else
            {
                var fp = Path.Combine(PresetDirectory, $"{name}.json");
                try
                {
                    using var stream = File.OpenRead(fp);
                    if (JsonSerializer.Deserialize(stream, SerializeInfo.Type, SerializeInfo.Context) is ConvertConfig loaded && PropertiesEqual(this, loaded))
                        return name;
                }
                catch (JsonException) { }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
        return null;
    }
}
