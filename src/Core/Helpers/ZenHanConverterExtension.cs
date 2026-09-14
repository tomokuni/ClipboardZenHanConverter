using EsUtil.ClipboardZenHanConverter.Core.Enums;
using EsUtil.Helper.ZenHanConverter;
using static EsUtil.Helper.ZenHanConverter.Define;

namespace EsUtil.ClipboardZenHanConverter.Core.Helpers;

/// <summary>列挙型の変換モードに基づいて EsUtil の ConvertPairs を解決する拡張メソッドを提供します。</summary>
/// <remarks>CharConverter.ResolvePairs から呼び出され、モード種別ごとに適切な変換ペアを生成します。<br/>
/// 拡張メソッドとして実装することで、switch 式での型パターンマッチングと組み合わせて<br/>
/// 宣言的かつDRYなコードを実現しています。</remarks>
public static class ZenHanConverterExtension
{
    /// <summary>ZenHanMode に基づいて IZenHanConverterToHanToZen の変換ペアを解決します。</summary>
    /// <param name="mode">変換モード（ToHan / ToZen / None）</param>
    /// <param name="entry">EsUtil の双方向変換エントリ</param>
    /// <returns>解決された変換ペア</returns>
    public static ConvertPairs GetConvertPairs(this ZenHanMode mode, IZenHanConverterToHanToZen entry)
        => mode switch
        {
            ZenHanMode.ToHan => entry.ToHanMap,
            ZenHanMode.ToZen => entry.ToZenMap,
            _ => ConvertPairs.Empty,
        };

    /// <summary>ZenHanKanaMode に基づいて IZenHanConverterToHanToZen の変換ペアを解決します。</summary>
    /// <param name="mode">変換モード（ToHan / ToZenKata / ToZenHira / None）</param>
    /// <param name="entry">EsUtil の双方向変換エントリ</param>
    /// <returns>解決された変換ペア</returns>
    /// <remarks>ToZenKata は entry.ToZenMap、ToZenHira は entry.ToHanMap（＝全角かな→ひらがな）として扱われます。</remarks>
    public static ConvertPairs GetConvertPairs(this ZenHanKanaMode mode, IZenHanConverterToHanToZen entry)
        => mode switch
        {
            ZenHanKanaMode.ToHan => entry.ToHanMap,
            ZenHanKanaMode.ToZenKata => entry.ToZenMap,
            ZenHanKanaMode.ToZenHira => entry.ToHanMap,
            _ => ConvertPairs.Empty,
        };

    /// <summary>ZenHanEtcZenHanAsciiMode に基づいて変換ペアを解決します。</summary>
    /// <param name="mode">変換モード（ToHan / ToZen / ToAscii / None）</param>
    /// <param name="entry">EsUtil の変換エントリ（IZenHanConverterToHan / ToZen / ToAscii）</param>
    /// <returns>解決された変換ペア</returns>
    /// <remarks>entry のランタイム型に応じて ToHanMap / ToZenMap / ToAsciiMap を使い分けます。</remarks>
    public static ConvertPairs GetConvertPairs(this ZenHanEtcZenHanAsciiMode mode, object entry)
        => mode switch
        {
            ZenHanEtcZenHanAsciiMode.ToHan when entry is IZenHanConverterToHan h => h.ToHanMap,
            ZenHanEtcZenHanAsciiMode.ToZen when entry is IZenHanConverterToZen z => z.ToZenMap,
            ZenHanEtcZenHanAsciiMode.ToAscii when entry is IZenHanConverterToAscii a => a.ToAsciiMap,
            _ => ConvertPairs.Empty,
        };
}
