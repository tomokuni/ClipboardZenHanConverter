using EsUtil.ZenHanConverter.App.Core.Enums;
using EsUtil.ZenHanConverter.App.Core.Helpers;
using EsUtil.Feature.ZenHan;
using System;
using System.Linq;
using Xunit;

namespace EsUtil.ZenHanConverter.Tests.App.Core.Helpers;

/// <summary><see cref="ZenHanConverterExtension"/> のモード別ペア解決を検証します。</summary>
public sealed class ZenHanConverterExtensionTests
{
    [Theory]
    [InlineData(ZenHanMode.None)]
    [InlineData(ZenHanKanaMode.None)]
    [InlineData(ZenHanEtcZenHanAsciiMode.None)]
    internal void GetConvertPairs_Noneは空のペアを返す(Enum mode)
    {
        var result = mode switch
        {
            ZenHanMode m => m.GetConvertPairs(GroupOf.Ascii.Numeric),
            ZenHanKanaMode m => m.GetConvertPairs(NameOf.Kana.GA),
            ZenHanEtcZenHanAsciiMode m => m.GetConvertPairs(NameOf.Kana.Prolong),
            _ => ConvertPairs.Empty,
        };

        Assert.Empty(result);
    }

    [Fact]
    internal void GetConvertPairs_ZenHanMode_ToHanは変換ペアを返す()
    {
        Assert.NotEmpty(ZenHanMode.ToHan.GetConvertPairs(GroupOf.Ascii.Numeric));
    }

    [Fact]
    internal void GetConvertPairs_ZenHanMode_ToZenは変換ペアを返す()
    {
        Assert.NotEmpty(ZenHanMode.ToZen.GetConvertPairs(GroupOf.Ascii.Numeric));
    }

    [Fact]
    internal void GetConvertPairs_ZenHanMode_ToHanとToZenは異なるペアを返す()
    {
        var toHan = ZenHanMode.ToHan.GetConvertPairs(GroupOf.Ascii.Numeric).ToList();
        var toZen = ZenHanMode.ToZen.GetConvertPairs(GroupOf.Ascii.Numeric).ToList();

        Assert.NotEqual(toHan, toZen);
    }

    [Fact]
    internal void GetConvertPairs_ZenHanKanaMode_ToHanは変換ペアを返す()
    {
        Assert.NotEmpty(ZenHanKanaMode.ToHan.GetConvertPairs(NameOf.Kana.GA));
    }

    [Fact]
    internal void GetConvertPairs_ZenHanKanaMode_ToZenKataとToZenHiraは変換ペアを返す()
    {
        Assert.NotEmpty(ZenHanKanaMode.ToZenKata.GetConvertPairs(NameOf.Kana.GA));
        Assert.NotEmpty(ZenHanKanaMode.ToZenHira.GetConvertPairs(NameOf.Kana.GA));
    }

    [Theory]
    [InlineData(ZenHanEtcZenHanAsciiMode.ToHan)]
    [InlineData(ZenHanEtcZenHanAsciiMode.ToZen)]
    [InlineData(ZenHanEtcZenHanAsciiMode.ToAscii)]
    internal void GetConvertPairs_ZenHanEtcZenHanAsciiMode_未知のエントリ型は空を返す(ZenHanEtcZenHanAsciiMode mode)
    {
        var result = mode.GetConvertPairs(new object());

        Assert.Empty(result);
    }
}
