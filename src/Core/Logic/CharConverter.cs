using System.Text.RegularExpressions;
using ClipboardZenHanConverter.Core.Enums;
using ClipboardZenHanConverter.Core.Helpers;
using ClipboardZenHanConverter.Core.Models;
using EsUtil.Helper.ZenHanConverter;
using static EsUtil.Helper.ZenHanConverter.Define;

namespace ClipboardZenHanConverter.Core.Logic;

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

    private void OnConfigPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) => _cachedPairs = null;

    public void Dispose()
    {
        if (_disposed) return;
        Config.PropertyChanged -= OnConfigPropertyChanged;
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    public ConvertPairs GetConvertPairs()
    {
        if (_cachedPairs != null) return _cachedPairs;
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

    private ConvertPairs GetNumberPairs() => Config.ConvertModeNumber.GetConvertPairs(GroupOf.Ascii.Numeric);
    private ConvertPairs GetAlphabetPairs() => Config.ConvertModeAlphabet.GetConvertPairs(GroupOf.Ascii.Alphabet);

    private ConvertPairs GetAsciiSymbolPairs()
    {
        static ConvertPairs Get(ZenHanMode m, IZenHanConverterToHanToZen e) => m.GetConvertPairs(e);
        return ConvertPairs.Concat(
            Get(Config.ConvertModeSymbolParenthesis, NameOf.Ascii.ParenthesisLeft),
            Get(Config.ConvertModeSymbolParenthesis, NameOf.Ascii.ParenthesisRight),
            Get(Config.ConvertModeSymbolSquareBracket, NameOf.Ascii.SquareBracketLeft),
            Get(Config.ConvertModeSymbolSquareBracket, NameOf.Ascii.SquareBracketRight),
            Get(Config.ConvertModeSymbolCurlyBracket, NameOf.Ascii.CurlyBracketLeft),
            Get(Config.ConvertModeSymbolCurlyBracket, NameOf.Ascii.CurlyBracketRight),
            Get(Config.ConvertModeSymbolDoubleQuote, NameOf.Ascii.DoubleQuote),
            Get(Config.ConvertModeSymbolSingleQuote, NameOf.Ascii.SingleQuote),
            Get(Config.ConvertModeSymbolComma, NameOf.Ascii.Comma),
            Get(Config.ConvertModeSymbolPeriod, NameOf.Ascii.Period),
            Get(Config.ConvertModeSymbolColon, NameOf.Ascii.Colon),
            Get(Config.ConvertModeSymbolSemicolon, NameOf.Ascii.Semicolon),
            Get(Config.ConvertModeSymbolLessThan, NameOf.Ascii.LessThan),
            Get(Config.ConvertModeSymbolEqual, NameOf.Ascii.Equal),
            Get(Config.ConvertModeSymbolGreaterThan, NameOf.Ascii.GreaterThan),
            Get(Config.ConvertModeSymbolPlus, NameOf.Ascii.Plus),
            Get(Config.ConvertModeSymbolHyphenMinus, NameOf.Ascii.HyphenMinus),
            Get(Config.ConvertModeSymbolExclamation, NameOf.Ascii.Exclamation),
            Get(Config.ConvertModeSymbolSharp, NameOf.Ascii.Sharp),
            Get(Config.ConvertModeSymbolDollar, NameOf.Ascii.Dollar),
            Get(Config.ConvertModeSymbolPercent, NameOf.Ascii.Percent),
            Get(Config.ConvertModeSymbolAmpersand, NameOf.Ascii.Ampersand),
            Get(Config.ConvertModeSymbolAsterisk, NameOf.Ascii.Asterisk),
            Get(Config.ConvertModeSymbolSlash, NameOf.Ascii.Slash),
            Get(Config.ConvertModeSymbolQuestion, NameOf.Ascii.Question),
            Get(Config.ConvertModeSymbolAt, NameOf.Ascii.At),
            Get(Config.ConvertModeSymbolCaret, NameOf.Ascii.Caret),
            Get(Config.ConvertModeSymbolUnderBar, NameOf.Ascii.UnderBar),
            Get(Config.ConvertModeSymbolBackquote, NameOf.Ascii.Backquote),
            Get(Config.ConvertModeSymbolVerticalBar, NameOf.Ascii.VerticalBar));
    }

    private ConvertPairs GetKanaPairs() => ConvertPairs.Concat(
        Config.ConvertModeKanaHan switch { ZenHanKanaMode.ToZenKata => ConvertPairs.Concat(GroupOf.Kana.Kata.ToZenMap), ZenHanKanaMode.ToZenHira => ConvertPairs.Concat(GroupOf.Kana.Hira.ToZenMap), _ => ConvertPairs.Empty },
        Config.ConvertModeKanaZenKata switch { ZenHanKanaMode.ToHan => ConvertPairs.Concat(GroupOf.Kana.Kata.ToHanMap), ZenHanKanaMode.ToZenHira => ConvertPairs.Concat(GroupOf.Kana.ToHiraMap), _ => ConvertPairs.Empty },
        Config.ConvertModeKanaZenHira switch { ZenHanKanaMode.ToHan => ConvertPairs.Concat(GroupOf.Kana.Hira.ToHanMap), ZenHanKanaMode.ToZenHira => ConvertPairs.Concat(GroupOf.Kana.ToKataMap), _ => ConvertPairs.Empty });

    private ConvertPairs GetKanaSymbolPairs() => ConvertPairs.Concat(
        Config.ConvertModeEtcKanaVoice.GetConvertPairs(NameOf.Kana.Voice),
        Config.ConvertModeEtcKanaSemiVoice.GetConvertPairs(NameOf.Kana.SemiVoice),
        Config.ConvertModeEtcKanaMiddleDot.GetConvertPairs(NameOf.Kana.MiddleDot),
        Config.ConvertModeEtcKanaLeftCornerBracket.GetConvertPairs(NameOf.Kana.LeftCornerBracket),
        Config.ConvertModeEtcKanaRightCornerBracket.GetConvertPairs(NameOf.Kana.RightCornerBracket));

    private ConvertPairs GetKanaEtcPairs() => ConvertPairs.Concat(
        Config.ConvertModeEtcKanaProlong.GetConvertPairs(NameOf.Kana.Prolong),
        Config.ConvertModeEtcKanaPeriod.GetConvertPairs(NameOf.Kana.Period),
        Config.ConvertModeEtcKanaComma.GetConvertPairs(NameOf.Kana.Comma));

    private ConvertPairs GetYenPairs() => ConvertPairs.Concat(
        GetYenConvertPairs(Config.ConvertModeEtcBSlashHan, "\\"),
        GetYenConvertPairs(Config.ConvertModeEtcBSlashZen, "＼"),
        GetYenConvertPairs(Config.ConvertModeEtcYenHan, "¥"),
        GetYenConvertPairs(Config.ConvertModeEtcYenZen, "￥"));

    private static ConvertPairs GetYenConvertPairs(ZenHanEtcYenMode mode, string src) => mode switch
    {
        ZenHanEtcYenMode.ToHanBSlash => new ConvertPairs([(src, "\\")]),
        ZenHanEtcYenMode.ToZenBSlash => new ConvertPairs([(src, "＼")]),
        ZenHanEtcYenMode.ToHanYen => new ConvertPairs([(src, "¥")]),
        ZenHanEtcYenMode.ToZenYen => new ConvertPairs([(src, "￥")]),
        _ => ConvertPairs.Empty
    };

    private ConvertPairs GetTabSpacePairs() => Config.ConvertModeEtcTabSpace switch
    {
        ZenHanEtcSpecial.ToHanSpace => new ConvertPairs([("\t", " ")]),
        ZenHanEtcSpecial.ToZenSpace => new ConvertPairs([("\t", "　")]),
        ZenHanEtcSpecial.Remove => new ConvertPairs([("\t", "")]),
        _ => ConvertPairs.Empty
    };

    private ConvertPairs GetNewlinePairs() => Config.ConvertModeEtcNewline switch
    {
        ZenHanEtcSpecial.ToHanSpace => new ConvertPairs([("\r\n", " "), ("\n", " "), ("\r", " ")]),
        ZenHanEtcSpecial.ToZenSpace => new ConvertPairs([("\r\n", "　"), ("\n", "　"), ("\r", "　")]),
        ZenHanEtcSpecial.Remove => new ConvertPairs([("\r\n", ""), ("\n", ""), ("\r", "")]),
        _ => ConvertPairs.Empty
    };

    public string Convert(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        if (Config.IsEnabledZenHan)
        {
            text = EsUtil.Helper.ZenHanConverter.ZenHanConverter.ToNormalize(text);
            text = GetConvertPairs().Convert(text);
            text = Config.ConvertModeEtcMultiSpace switch
            {
                ZenHanEtcSpecial.ToHanSpace => SingleSpaceRegex().Replace(text, " "),
                ZenHanEtcSpecial.ToZenSpace => SingleSpaceRegex().Replace(text, "　"),
                ZenHanEtcSpecial.Remove => SingleSpaceRegex().Replace(text, ""),
                _ => text
            };
        }
        return ApplyUserReplacements(text);
    }

    private string ApplyUserReplacements(string text)
    {
        foreach (var pair in Config.ReplacePairs)
        {
            if (string.IsNullOrEmpty(pair.Search)) continue;
            try
            {
                text = pair.IsRegex ? Regex.Replace(text, pair.Search, pair.Replace)
                    : text.Replace(pair.Search, pair.Replace, StringComparison.Ordinal);
            }
            catch { }
        }
        return text;
    }

    [GeneratedRegex("[ 　]{2,}")]
    private static partial Regex SingleSpaceRegex();
}
