using EsUtil.ClipboardZenHanConverter.Core.Enums;

namespace EsUtil.ClipboardZenHanConverter.Core.Models;

/// <summary>設定画面のセグメントコントロール定義を提供する静的クラス。</summary>
/// <remarks>提供機能: <br/>
/// - 各変換カテゴリ（数字/英字/かな/記号/約物/BS/YEN/特殊）の SegmentDefine 配列の提供<br/><br/>
/// 特徴: <br/>
/// - ラベルとセグメント項目の定義を単一所有し、画面側での定義重複を排除<br/>
/// - UI フレームワークに依存しないため Core 層に配置し、MewUI / WinUI3 の双方で共有<br/>
/// - バインド先は ConvertConfig.Mode（型安全なアクセサ定義）を直接参照し、プロパティ名文字列による実行時名前解決を避ける（NativeAOT 対応）</remarks>
public static class SegmentDefinitions
{
    /// <summary>数字変換モードのセグメント定義配列。</summary>
    public static readonly SegmentDefine[] NumberDefs = [new("数字 の変換", ConvertConfig.Mode.Number)];

    /// <summary>英字変換モードのセグメント定義配列。</summary>
    public static readonly SegmentDefine[] AlphabetDefs = [new("英字 の変換", ConvertConfig.Mode.Alphabet)];

    /// <summary>かな変換モードのセグメント定義配列（半角カナ/全角カタカナ/全角ひらがな）。</summary>
    public static readonly SegmentDefine[] KanaDefs =
    [
        new("半角カナ の変換", ConvertConfig.Mode.KanaHan, Segments:
        [
            new("なし", ZenHanKanaMode.None),
            new("半角 カナ", ZenHanKanaMode.ToHan, IsEnabled: false),
            new("全角 カタカナ", ZenHanKanaMode.ToZenKata),
            new("全角 ひらがな", ZenHanKanaMode.ToZenHira),
        ]),
        new("全角カタカナ の変換", ConvertConfig.Mode.KanaZenKata, Segments:
        [
            new("なし", ZenHanKanaMode.None),
            new("半角 カナ", ZenHanKanaMode.ToHan),
            new("全角 カタカナ", ZenHanKanaMode.ToZenKata, IsEnabled: false),
            new("全角 ひらがな", ZenHanKanaMode.ToZenHira),
        ]),
        new("全角ひらがな の変換", ConvertConfig.Mode.KanaZenHira, Segments:
        [
            new("なし", ZenHanKanaMode.None),
            new("半角 カナ", ZenHanKanaMode.ToHan),
            new("全角 カタカナ", ZenHanKanaMode.ToZenKata),
            new("全角 ひらがな", ZenHanKanaMode.ToZenHira, IsEnabled: false),
        ]),
    ];

    /// <summary>記号変換モードのセグメント定義配列（括弧/引用符/演算子等）。</summary>
    public static readonly SegmentDefine[] SymbolDefs =
    [
        new("丸括弧　（）", ConvertConfig.Mode.SymbolParenthesis),
        new("角括弧　［］", ConvertConfig.Mode.SymbolSquareBracket),
        new("波括弧　｛｝", ConvertConfig.Mode.SymbolCurlyBracket),
        new("ダブルクォート　\"", ConvertConfig.Mode.SymbolDoubleQuote),
        new("シングルクォート　'", ConvertConfig.Mode.SymbolSingleQuote),
        new("カンマ　，", ConvertConfig.Mode.SymbolComma),
        new("ピリオド　．", ConvertConfig.Mode.SymbolPeriod),
        new("コロン　：", ConvertConfig.Mode.SymbolColon),
        new("セミコロン　；", ConvertConfig.Mode.SymbolSemicolon),
        new("不等号　＜", ConvertConfig.Mode.SymbolLessThan),
        new("イコール　＝", ConvertConfig.Mode.SymbolEqual),
        new("不等号　＞", ConvertConfig.Mode.SymbolGreaterThan),
        new("プラス　＋", ConvertConfig.Mode.SymbolPlus),
        new("マイナス　－", ConvertConfig.Mode.SymbolHyphenMinus),
        new("はてな　？", ConvertConfig.Mode.SymbolQuestion),
        new("びっくり　！", ConvertConfig.Mode.SymbolExclamation),
        new("シャープ　＃", ConvertConfig.Mode.SymbolSharp),
        new("ダラー　＄", ConvertConfig.Mode.SymbolDollar),
        new("パーセント　％", ConvertConfig.Mode.SymbolPercent),
        new("アンパサンド　＆", ConvertConfig.Mode.SymbolAmpersand),
        new("アスタリスク　＊", ConvertConfig.Mode.SymbolAsterisk),
        new("スラッシュ　／", ConvertConfig.Mode.SymbolSlash),
        new("アットマーク　＠", ConvertConfig.Mode.SymbolAt),
        new("キャレット　＾", ConvertConfig.Mode.SymbolCaret),
        new("アンダースコア　＿", ConvertConfig.Mode.SymbolUnderBar),
        new("バッククォート　`", ConvertConfig.Mode.SymbolBackquote),
        new("縦棒　｜", ConvertConfig.Mode.SymbolVerticalBar),
        new("チルダ　～", ConvertConfig.Mode.SymbolTilde),
        new("スペース　⬚", ConvertConfig.Mode.SymbolSpace),
        new("かな 濁点　゛", ConvertConfig.Mode.EtcKanaVoice),
        new("かな 半濁点　゜", ConvertConfig.Mode.EtcKanaSemiVoice),
        new("かな 中点　・", ConvertConfig.Mode.EtcKanaMiddleDot),
        new("かな 左上括弧　「", ConvertConfig.Mode.EtcKanaLeftCornerBracket),
        new("かな 右下括弧　」", ConvertConfig.Mode.EtcKanaRightCornerBracket),
    ];

    /// <summary>約物（長音/読点/句点）変換モードのセグメント定義配列。</summary>
    public static readonly SegmentDefine[] EtcZenHanAsciiDefs =
    [
        new("かな 長音記号　ー", ConvertConfig.Mode.EtcKanaProlong, Segments:
        [
            new("なし", ZenHanEtcZenHanAsciiMode.None),
            new("半角 ｰ", ZenHanEtcZenHanAsciiMode.ToHan),
            new("全角 ー", ZenHanEtcZenHanAsciiMode.ToZen),
            new("Ascii -", ZenHanEtcZenHanAsciiMode.ToAscii),
        ]),
        new("かな 読点　。", ConvertConfig.Mode.EtcKanaPeriod, Segments:
        [
            new("なし", ZenHanEtcZenHanAsciiMode.None),
            new("半角 ｡", ZenHanEtcZenHanAsciiMode.ToHan),
            new("全角 。", ZenHanEtcZenHanAsciiMode.ToZen),
            new("Ascii .", ZenHanEtcZenHanAsciiMode.ToAscii),
        ]),
        new("かな 句点　、", ConvertConfig.Mode.EtcKanaComma, Segments:
        [
            new("なし", ZenHanEtcZenHanAsciiMode.None),
            new("半角 ､", ZenHanEtcZenHanAsciiMode.ToHan),
            new("全角 、", ZenHanEtcZenHanAsciiMode.ToZen),
            new("Ascii ,", ZenHanEtcZenHanAsciiMode.ToAscii),
        ]),
    ];

    /// <summary>バックスラッシュ/円記号変換モードのセグメント定義配列。</summary>
    public static readonly SegmentDefine[] EtcBslashYenDefs =
    [
        new("バックスラッシュ 半角", ConvertConfig.Mode.EtcBSlashHan, Segments:
        [
            new("なし", ZenHanEtcYenMode.None),
            new("半角 \\", ZenHanEtcYenMode.ToHanBSlash, IsEnabled: false),
            new("全角 ＼", ZenHanEtcYenMode.ToZenBSlash),
            new("半角 ¥", ZenHanEtcYenMode.ToHanYen),
            new("全角 ￥", ZenHanEtcYenMode.ToZenYen),
        ]),
        new("バックスラッシュ 全角", ConvertConfig.Mode.EtcBSlashZen, Segments:
        [
            new("なし", ZenHanEtcYenMode.None),
            new("半角 \\", ZenHanEtcYenMode.ToHanBSlash),
            new("全角 ＼", ZenHanEtcYenMode.ToZenBSlash, IsEnabled: false),
            new("半角 ¥", ZenHanEtcYenMode.ToHanYen),
            new("全角 ￥", ZenHanEtcYenMode.ToZenYen),
        ]),
        new("円記号 半角", ConvertConfig.Mode.EtcYenHan, Segments:
        [
            new("なし", ZenHanEtcYenMode.None),
            new("半角 \\", ZenHanEtcYenMode.ToHanBSlash),
            new("全角 ＼", ZenHanEtcYenMode.ToZenBSlash),
            new("半角 ¥", ZenHanEtcYenMode.ToHanYen, IsEnabled: false),
            new("全角 ￥", ZenHanEtcYenMode.ToZenYen),
        ]),
        new("円記号 全角", ConvertConfig.Mode.EtcYenZen, Segments:
        [
            new("なし", ZenHanEtcYenMode.None),
            new("半角 \\", ZenHanEtcYenMode.ToHanBSlash),
            new("全角 ＼", ZenHanEtcYenMode.ToZenBSlash),
            new("半角 ¥", ZenHanEtcYenMode.ToHanYen),
            new("全角 ￥", ZenHanEtcYenMode.ToZenYen, IsEnabled: false),
        ]),
    ];

    /// <summary>特殊文字（タブ/改行）変換モードのセグメント定義配列。</summary>
    public static readonly SegmentDefine[] EtcSpecialDefs =
    [
        new("タブ記号", ConvertConfig.Mode.EtcTabSpace, Segments:
        [
            new("そのまま", ZenHanEtcSpecial.None),
            new("単純除去", ZenHanEtcSpecial.Remove),
            new("半角スペース", ZenHanEtcSpecial.ToHanSpace),
            new("全角スペース", ZenHanEtcSpecial.ToZenSpace),
        ]),
        new("改行記号", ConvertConfig.Mode.EtcNewline, Segments:
        [
            new("そのまま", ZenHanEtcSpecial.None),
            new("単純除去", ZenHanEtcSpecial.Remove),
            new("半角スペース", ZenHanEtcSpecial.ToHanSpace),
            new("全角スペース", ZenHanEtcSpecial.ToZenSpace),
        ]),
    ];

    /// <summary>連続スペース変換モードのセグメント定義配列。</summary>
    public static readonly SegmentDefine[] EtcMultiSpaceDefs =
    [
        new("連続スペース", ConvertConfig.Mode.EtcMultiSpace, Height: 80, Segments:
        [
            new("そのまま", ZenHanEtcSpecial.None),
            new("単純除去", ZenHanEtcSpecial.Remove),
            new("単一半角スペース", ZenHanEtcSpecial.ToHanSpace),
            new("単一全角スペース", ZenHanEtcSpecial.ToZenSpace),
        ]),
    ];
}
