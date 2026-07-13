using ClipboardZenHanConverter.Core.Enums;

namespace ClipboardZenHanConverter.Core.Models;


/// <summary>セグメント項目のデータを保持する record です。</summary>
public record SegmentItem(string Content, object Value, bool IsEnabled = true);

public record SegmentDefine(string Label, string Prop, double Height = double.NaN, SegmentItem[]? Segments = null, bool? ForceEnableState = null);

/// <summary>文字列置換のペアを表す record です。</summary>
public record ReplacePair(string Search, string Replace, bool IsRegex = false);


public partial class SettingsModel
{
    public const string TextNone = "なし";
    public const string TextToHan = "半角";
    public const string TextToZen = "全角";

    public const string TextKanaNone = "なし";
    public const string TextKanaToHan = "半角カナ";
    public const string TextKanaToZenKata = "全角カタカナ";
    public const string TextKanaToZenHira = "全角ひらがな";

    public SettingsModel()
    {
    }

    public readonly SegmentDefine[] NumberDefs =
    [
        new("数字 の変換", nameof(ConvertConfig.ConvertModeNumber)),
    ];

    public readonly SegmentDefine[] AlphabetDefs =
    [
        new("英字 の変換", nameof(ConvertConfig.ConvertModeAlphabet)),
    ];

    public readonly SegmentDefine[] KanaDefs =
    [
        new("半角カナ の変換", nameof(ConvertConfig.ConvertModeKanaHan), Segments:
        [
            new("なし", ZenHanKanaMode.None),
            new("半角 カナ", ZenHanKanaMode.ToHan, IsEnabled: false),
            new("全角 カタカナ", ZenHanKanaMode.ToZenKata),
            new("全角 ひらがな", ZenHanKanaMode.ToZenHira),
        ]),
        new("全角カタカナ の変換", nameof(ConvertConfig.ConvertModeKanaZenKata), Segments:
        [
            new("なし", ZenHanKanaMode.None),
            new("半角 カナ", ZenHanKanaMode.ToHan),
            new("全角 カタカナ", ZenHanKanaMode.ToZenKata, IsEnabled: false),
            new("全角 ひらがな", ZenHanKanaMode.ToZenHira),
        ]),
        new ("全角ひらがな の変換", nameof(ConvertConfig.ConvertModeKanaZenHira), Segments:
        [
            new("なし", ZenHanKanaMode.None),
            new("半角 カナ", ZenHanKanaMode.ToHan),
            new("全角 カタカナ", ZenHanKanaMode.ToZenKata),
            new("全角 ひらがな", ZenHanKanaMode.ToZenHira, IsEnabled: false),
        ]),
    ];

    public readonly SegmentDefine[] SymbolDefs =
    [
        new("丸括弧　（）", nameof(ConvertConfig.ConvertModeSymbolParenthesis)),
        new("角括弧　［］", nameof(ConvertConfig.ConvertModeSymbolSquareBracket)),
        new("波括弧　｛｝", nameof(ConvertConfig.ConvertModeSymbolCurlyBracket)),
        new("ダブルクォート　”", nameof(ConvertConfig.ConvertModeSymbolDoubleQuote)),
        new("シングルクォート　’", nameof(ConvertConfig.ConvertModeSymbolSingleQuote)),
        new("カンマ　，", nameof(ConvertConfig.ConvertModeSymbolComma)),
        new("ピリオド　．", nameof(ConvertConfig.ConvertModeSymbolPeriod)),
        new("コロン　：", nameof(ConvertConfig.ConvertModeSymbolColon)),
        new("セミコロン　；", nameof(ConvertConfig.ConvertModeSymbolSemicolon)),
        new("不等号　＜", nameof(ConvertConfig.ConvertModeSymbolLessThan)),
        new("イコール　＝", nameof(ConvertConfig.ConvertModeSymbolEqual)),
        new("不等号　＞", nameof(ConvertConfig.ConvertModeSymbolGreaterThan)),
        new("プラス　＋", nameof(ConvertConfig.ConvertModeSymbolPlus)),
        new("マイナス　－", nameof(ConvertConfig.ConvertModeSymbolHyphenMinus)),
        new("はてな　？", nameof(ConvertConfig.ConvertModeSymbolQuestion)),
        new("びっくり　！", nameof(ConvertConfig.ConvertModeSymbolExclamation)),
        new("シャープ　＃", nameof(ConvertConfig.ConvertModeSymbolSharp)),
        new("ダラー　＄", nameof(ConvertConfig.ConvertModeSymbolDollar)),
        new("パーセント　％", nameof(ConvertConfig.ConvertModeSymbolPercent)),
        new("アンパサンド　＆", nameof(ConvertConfig.ConvertModeSymbolAmpersand)),
        new("アスタリスク　＊", nameof(ConvertConfig.ConvertModeSymbolAsterisk)),
        new("スラッシュ　／", nameof(ConvertConfig.ConvertModeSymbolSlash)),
        new("アットマーク　＠", nameof(ConvertConfig.ConvertModeSymbolAt)),
        new("キャレット　＾", nameof(ConvertConfig.ConvertModeSymbolCaret)),
        new("アンダースコア　＿", nameof(ConvertConfig.ConvertModeSymbolUnderBar)),
        new("バッククォート　`", nameof(ConvertConfig.ConvertModeSymbolBackquote)),
        new("縦棒　｜", nameof(ConvertConfig.ConvertModeSymbolVerticalBar)),
        new("チルダ　～", nameof(ConvertConfig.ConvertModeSymbolTilde)),
        new("スペース　⬚", nameof(ConvertConfig.ConvertModeSymbolSpace)),
        new("かな 濁点　゛", nameof(ConvertConfig.ConvertModeEtcKanaVoice)),
        new("かな 半濁点　゜", nameof(ConvertConfig.ConvertModeEtcKanaSemiVoice)),
        new("かな 中点　・", nameof(ConvertConfig.ConvertModeEtcKanaMiddleDot)),
        new("かな 左上括弧　「", nameof(ConvertConfig.ConvertModeEtcKanaLeftCornerBracket)),
        new("かな 右下括弧　」", nameof(ConvertConfig.ConvertModeEtcKanaRightCornerBracket)),
    ];

    public readonly SegmentDefine[] EtcZenHanAsciiDefs =
    [
        new ("かな 長音記号　ー", nameof(ConvertConfig.ConvertModeEtcKanaProlong), Segments:
        [
            new("なし", ZenHanEtcZenHanAsciiMode.None),
            new("半角 ｰ", ZenHanEtcZenHanAsciiMode.ToHan),
            new("全角 ー", ZenHanEtcZenHanAsciiMode.ToZen),
            new("Ascii -", ZenHanEtcZenHanAsciiMode.ToAscii),
        ]),
        new ("かな 読点　。", nameof(ConvertConfig.ConvertModeEtcKanaPeriod), Segments:
        [
            new("なし", ZenHanEtcZenHanAsciiMode.None),
            new("半角 ｡", ZenHanEtcZenHanAsciiMode.ToHan),
            new("全角 。", ZenHanEtcZenHanAsciiMode.ToZen),
            new("Ascii .", ZenHanEtcZenHanAsciiMode.ToAscii),
        ]),
        new ("かな 句点　、", nameof(ConvertConfig.ConvertModeEtcKanaComma), Segments:
        [
            new("なし", ZenHanEtcZenHanAsciiMode.None),
            new("半角 ､", ZenHanEtcZenHanAsciiMode.ToHan),
            new("全角 、", ZenHanEtcZenHanAsciiMode.ToZen),
            new("Ascii ,", ZenHanEtcZenHanAsciiMode.ToAscii),
        ]),
    ];

    public readonly SegmentDefine[] EtcBslashYenDefs =
    [
        new("バックスラッシュ 半角　＼", nameof(ConvertConfig.ConvertModeEtcBSlashHan), Segments:
        [
            new("なし", ZenHanEtcYenMode.None),
            new("半角 ＼", ZenHanEtcYenMode.ToHanBSlash, IsEnabled: false),
            new("全角 ＼", ZenHanEtcYenMode.ToZenBSlash),
            new("半角 ￥", ZenHanEtcYenMode.ToHanYen),
            new("全角 ￥", ZenHanEtcYenMode.ToZenYen),
        ]),
        new("バックスラッシュ 全角　＼", nameof(ConvertConfig.ConvertModeEtcBSlashZen), Segments:
        [
            new("なし", ZenHanEtcYenMode.None),
            new("半角 ＼", ZenHanEtcYenMode.ToHanBSlash),
            new("全角 ＼", ZenHanEtcYenMode.ToZenBSlash, IsEnabled: false),
            new("半角 ￥", ZenHanEtcYenMode.ToHanYen),
            new("全角 ￥", ZenHanEtcYenMode.ToZenYen),
        ]),
        new("円記号 半角　￥", nameof(ConvertConfig.ConvertModeEtcYenHan), Segments:
        [
            new("なし", ZenHanEtcYenMode.None),
            new("半角 ＼", ZenHanEtcYenMode.ToHanBSlash),
            new("全角 ＼", ZenHanEtcYenMode.ToZenBSlash),
            new("半角 ￥", ZenHanEtcYenMode.ToHanYen, IsEnabled: false),
            new("全角 ￥", ZenHanEtcYenMode.ToZenYen),
        ]),
        new("円記号 全角　￥", nameof(ConvertConfig.ConvertModeEtcYenZen), Segments:
        [
            new("なし", ZenHanEtcYenMode.None),
            new("半角 ＼", ZenHanEtcYenMode.ToHanBSlash),
            new("全角 ＼", ZenHanEtcYenMode.ToZenBSlash),
            new("半角 ￥", ZenHanEtcYenMode.ToHanYen),
            new("全角 ￥", ZenHanEtcYenMode.ToZenYen, IsEnabled: false),
        ]),
    ];

    public readonly SegmentDefine[] EtcSpecialDefs =
    [
        new("タブ記号", nameof(ConvertConfig.ConvertModeEtcTabSpace), Segments:
        [
            new("そのまま", ZenHanEtcSpecial.None),
            new("単純除去", ZenHanEtcSpecial.Remove),
            new("半角スペース", ZenHanEtcSpecial.ToHanSpace),
            new("全角スペース", ZenHanEtcSpecial.ToZenSpace),
        ]),
        new("改行記号", nameof(ConvertConfig.ConvertModeEtcNewline), Segments:
        [
            new("そのまま", ZenHanEtcSpecial.None),
            new("単純除去", ZenHanEtcSpecial.Remove),
            new("半角スペース", ZenHanEtcSpecial.ToHanSpace),
            new("全角スペース", ZenHanEtcSpecial.ToZenSpace),
        ]),
    ];

    public readonly SegmentDefine[] EtcMultiSpaceDefs =
    [
        new("連続スペース", nameof(ConvertConfig.ConvertModeEtcMultiSpace), Height: 80, Segments:
        [
            new("そのまま", ZenHanEtcSpecial.None),
            new("単純除去", ZenHanEtcSpecial.Remove),
            new("単一半角スペース", ZenHanEtcSpecial.ToHanSpace),
            new("単一全角スペース", ZenHanEtcSpecial.ToZenSpace),
        ]),
    ];

}
