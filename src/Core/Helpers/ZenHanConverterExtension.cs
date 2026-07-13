using EsUtil.Helper.ZenHanConverter;
using static EsUtil.Helper.ZenHanConverter.Define;
using ClipboardZenHanConverter.Core.Enums;

namespace ClipboardZenHanConverter.Core.Helpers;

public static partial class ZenHanConverterExtension
{
    public static ConvertPairs GetConvertPairs(this ZenHanMode mode, IZenHanConverterToHanToZen entry)
        => mode switch
        {
            ZenHanMode.ToHan => entry.ToHanMap,
            ZenHanMode.ToZen => entry.ToZenMap,
            _ => ConvertPairs.Empty,
        };

    public static ConvertPairs GetConvertPairs(this ZenHanKanaMode mode, IZenHanConverterToHanToZen entry)
        => mode switch
        {
            ZenHanKanaMode.ToHan => entry.ToHanMap,
            ZenHanKanaMode.ToZenKata => entry.ToZenMap,
            ZenHanKanaMode.ToZenHira => entry.ToHanMap,
            _ => ConvertPairs.Empty,
        };

    public static ConvertPairs GetConvertPairs(this ZenHanEtcZenHanAsciiMode mode, object entry)
        => mode switch
        {
            ZenHanEtcZenHanAsciiMode.ToHan when entry is IZenHanConverterToHan h => h.ToHanMap,
            ZenHanEtcZenHanAsciiMode.ToZen when entry is IZenHanConverterToZen z => z.ToZenMap,
            ZenHanEtcZenHanAsciiMode.ToAscii when entry is IZenHanConverterToAscii a => a.ToAsciiMap,
            _ => ConvertPairs.Empty,
        };
}
