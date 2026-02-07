using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ClipboardZenHanConverter.Core.Enums;
using ClipboardZenHanConverter.Core.Helpers;
using ClipboardZenHanConverter.Core.Models;
using EsUtil.Helper.ZenHanConverter;
using Microsoft.UI.Composition.Scenes;

namespace ClipboardZenHanConverter.Core.Logic;

/// <summary>文字変換ロジックを提供するクラスです。</summary>
/// <remarks>
/// ConvertConfig の設定に基づいて文字列を変換します。<br/>
/// IDisposable を実装し、イベント購読の解除を行います。<br/>
/// GetConvertPairs() メソッドにより、設定値から ConvertPairs オブジェクトを生成する。この計算コストを抑えるため、_cachedPairs フィールドに結果をキャッシュする。設定変更時にキャッシュは破棄される。<br/>
/// </remarks>
public partial class CharConverter : IDisposable
{
    public ConvertConfig Config { get; }

    private ConvertPairs? _cachedPairs;
    private bool _disposed;

    public CharConverter(ConvertConfig config)
    {
        Config = config;
        Config.PropertyChanged += OnConfigPropertyChanged;
    }

    private void OnConfigPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        _cachedPairs = null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            Config.PropertyChanged -= OnConfigPropertyChanged;
        }

        _disposed = true;
    }

    /// <summary>現在の設定に基づいて変換ペアを取得します。</summary>
    public ConvertPairs GetConvertPairs()
    {
        if (_cachedPairs != null)
            return _cachedPairs;

        var list = ConvertPairs.Empty;

        list = ConvertPairs.Concat(list, GetNumberPairs());
        list = ConvertPairs.Concat(list, GetAlphabetPairs());
        list = ConvertPairs.Concat(list, GetAsciiSymbolPairs());
        list = ConvertPairs.Concat(list, GetKanaPairs());
        list = ConvertPairs.Concat(list, GetKanaSymbolPairs());
        list = ConvertPairs.Concat(list, GetKanaEtcPairs());
        list = ConvertPairs.Concat(list, GetYenPairs());
        list = ConvertPairs.Concat(list, GetTabSpacePairs());
        list = ConvertPairs.Concat(list, GetNewlinePairs());

        _cachedPairs = new ConvertPairs(list);
        return _cachedPairs;
    }

    private ConvertPairs GetNumberPairs()
        => Config.ConvertModeNumber.GetConvertPairs(GroupOf.Ascii.Numeric);

    private ConvertPairs GetAlphabetPairs()
        => Config.ConvertModeAlphabet.GetConvertPairs(GroupOf.Ascii.Alphabet);

    private ConvertPairs GetAsciiSymbolPairs()
        => ConvertPairs.Concat(
            Config.ConvertModeSymbolParenthesis.GetConvertPairs(NameOf.Ascii.ParenthesisLeft),
            Config.ConvertModeSymbolParenthesis.GetConvertPairs(NameOf.Ascii.ParenthesisRight),
            Config.ConvertModeSymbolSquareBracket.GetConvertPairs(NameOf.Ascii.SquareBracketLeft),
            Config.ConvertModeSymbolSquareBracket.GetConvertPairs(NameOf.Ascii.SquareBracketRight),
            Config.ConvertModeSymbolCurlyBracket.GetConvertPairs(NameOf.Ascii.CurlyBracketLeft),
            Config.ConvertModeSymbolCurlyBracket.GetConvertPairs(NameOf.Ascii.CurlyBracketRight),
            Config.ConvertModeSymbolDoubleQuote.GetConvertPairs(NameOf.Ascii.DoubleQuote),
            Config.ConvertModeSymbolSingleQuote.GetConvertPairs(NameOf.Ascii.SingleQuote),
            Config.ConvertModeSymbolComma.GetConvertPairs(NameOf.Ascii.Comma),
            Config.ConvertModeSymbolPeriod.GetConvertPairs(NameOf.Ascii.Period),
            Config.ConvertModeSymbolColon.GetConvertPairs(NameOf.Ascii.Colon),
            Config.ConvertModeSymbolSemicolon.GetConvertPairs(NameOf.Ascii.Semicolon),
            Config.ConvertModeSymbolLessThan.GetConvertPairs(NameOf.Ascii.LessThan),
            Config.ConvertModeSymbolEqual.GetConvertPairs(NameOf.Ascii.Equal),
            Config.ConvertModeSymbolGreaterThan.GetConvertPairs(NameOf.Ascii.GreaterThan),
            Config.ConvertModeSymbolPlus.GetConvertPairs(NameOf.Ascii.Plus),
            Config.ConvertModeSymbolHyphenMinus.GetConvertPairs(NameOf.Ascii.HyphenMinus),
            Config.ConvertModeSymbolExclamation.GetConvertPairs(NameOf.Ascii.Exclamation),
            Config.ConvertModeSymbolSharp.GetConvertPairs(NameOf.Ascii.Sharp),
            Config.ConvertModeSymbolDollar.GetConvertPairs(NameOf.Ascii.Dollar),
            Config.ConvertModeSymbolPercent.GetConvertPairs(NameOf.Ascii.Percent),
            Config.ConvertModeSymbolAmpersand.GetConvertPairs(NameOf.Ascii.Ampersand),
            Config.ConvertModeSymbolAsterisk.GetConvertPairs(NameOf.Ascii.Asterisk),
            Config.ConvertModeSymbolSlash.GetConvertPairs(NameOf.Ascii.Slash),
            Config.ConvertModeSymbolQuestion.GetConvertPairs(NameOf.Ascii.Question),
            Config.ConvertModeSymbolAt.GetConvertPairs(NameOf.Ascii.At),
            Config.ConvertModeSymbolCaret.GetConvertPairs(NameOf.Ascii.Caret),
            Config.ConvertModeSymbolUnderBar.GetConvertPairs(NameOf.Ascii.UnderBar),
            Config.ConvertModeSymbolBackquote.GetConvertPairs(NameOf.Ascii.Backquote),
            Config.ConvertModeSymbolVerticalBar.GetConvertPairs(NameOf.Ascii.VerticalBar));

    private ConvertPairs GetKanaPairs()
        => ConvertPairs.Concat(
            Config.ConvertModeKanaHan switch
            {
                ZenHanKanaMode.ToZenKata => ConvertPairs.Concat(GroupOf.Kana.Kata.ToZenMap),
                ZenHanKanaMode.ToZenHira => ConvertPairs.Concat(GroupOf.Kana.Hira.ToZenMap),
                _ => ConvertPairs.Empty,
            },
            Config.ConvertModeKanaZenKata switch
            {
                ZenHanKanaMode.ToHan => ConvertPairs.Concat(GroupOf.Kana.Kata.ToHanMap),
                ZenHanKanaMode.ToZenHira => ConvertPairs.Concat(GroupOf.Kana.ToHiraMap),
                _ => ConvertPairs.Empty,
            },
            Config.ConvertModeKanaZenHira switch
            {
                ZenHanKanaMode.ToHan => ConvertPairs.Concat(GroupOf.Kana.Hira.ToHanMap),
                ZenHanKanaMode.ToZenHira => ConvertPairs.Concat(GroupOf.Kana.ToKataMap),
                _ => ConvertPairs.Empty,
            });

    private ConvertPairs GetKanaSymbolPairs()
        => ConvertPairs.Concat(
            Config.ConvertModeEtcKanaVoice.GetConvertPairs(NameOf.Kana.Voice),
            Config.ConvertModeEtcKanaSemiVoice.GetConvertPairs(NameOf.Kana.SemiVoice),
            Config.ConvertModeEtcKanaMiddleDot.GetConvertPairs(NameOf.Kana.MiddleDot),
            Config.ConvertModeEtcKanaLeftCornerBracket.GetConvertPairs(NameOf.Kana.LeftCornerBracket),
            Config.ConvertModeEtcKanaRightCornerBracket.GetConvertPairs(NameOf.Kana.RightCornerBracket));

    private ConvertPairs GetKanaEtcPairs()
        => ConvertPairs.Concat(
            Config.ConvertModeEtcKanaProlong.GetConvertPairs(NameOf.Kana.Prolong),
            Config.ConvertModeEtcKanaPeriod.GetConvertPairs(NameOf.Kana.Period),
            Config.ConvertModeEtcKanaComma.GetConvertPairs(NameOf.Kana.Comma));

    private ConvertPairs GetYenPairs()
        => ConvertPairs.Concat(
            GetYenConvertPairs(Config.ConvertModeEtcBSlashHan, "\\"),
            GetYenConvertPairs(Config.ConvertModeEtcBSlashZen, "＼"),
            GetYenConvertPairs(Config.ConvertModeEtcYenHan, "¥"),
            GetYenConvertPairs(Config.ConvertModeEtcYenZen, "￥"));

    private static ConvertPairs GetYenConvertPairs(ZenHanEtcYenMode mode, string source)
        => mode switch
        {
            ZenHanEtcYenMode.ToHanBSlash => new ConvertPairs([(source, "\\")]),
            ZenHanEtcYenMode.ToZenBSlash => new ConvertPairs([(source, "＼")]),
            ZenHanEtcYenMode.ToHanYen => new ConvertPairs([(source, "¥")]),
            ZenHanEtcYenMode.ToZenYen => new ConvertPairs([(source, "￥")]),
            _ => ConvertPairs.Empty
        };

    private ConvertPairs GetTabSpacePairs()
        => Config.ConvertModeEtcTabSpace switch
        {
            ZenHanEtcSpecial.ToHanSpace => new ConvertPairs([("\t", " ")]),
            ZenHanEtcSpecial.ToZenSpace => new ConvertPairs([("\t", "　")]),
            ZenHanEtcSpecial.Remove => new ConvertPairs([("\t", "")]),
            _ => ConvertPairs.Empty
        };

    private ConvertPairs GetNewlinePairs()
        => Config.ConvertModeEtcNewline switch
        {
            ZenHanEtcSpecial.ToHanSpace => new ConvertPairs([("\r\n", " "), ("\n", " "), ("\r", " ")]),
            ZenHanEtcSpecial.ToZenSpace => new ConvertPairs([("\r\n", "　"), ("\n", "　"), ("\r", "　")]),
            ZenHanEtcSpecial.Remove => new ConvertPairs([("\r\n", ""), ("\n", ""), ("\r", "")]),
            _ => ConvertPairs.Empty
        };

    /// <summary>文字列を変換します。</summary>
    /// <remarks>
    /// 正規化、文字置換、連続スペース処理を行います。<br/>
    /// </remarks>
    public string Convert(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        if (Config.IsEnabledZenHan)
        {
            // フリンジケースの空白やダッシュ表現、仮名の合成ルールを適用して文字列を正規化します。
            text = ZenHanConverter.ToNormalize(text);

            var converter = GetConvertPairs();
            text = converter.Convert(text);

            // 変換後に連続する空白を置換します。
            text = Config.ConvertModeEtcMultiSpace switch
            {
                ZenHanEtcSpecial.ToHanSpace => SingleSpaceRegex().Replace(text, " "),
                ZenHanEtcSpecial.ToZenSpace => SingleSpaceRegex().Replace(text, "　"),
                ZenHanEtcSpecial.Remove => SingleSpaceRegex().Replace(text, ""),
                _ => text
            };
        }

        return text;
    }

    [GeneratedRegex("[ 　]{2,}")]
    private static partial Regex SingleSpaceRegex();
}
