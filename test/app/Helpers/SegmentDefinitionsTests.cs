using ClipboardZenHanConverter.App.Helpers;
using Xunit;

namespace ClipboardZenHanConverter.Tests.App.Helpers;

public class SegmentDefinitionsTests
{
    [Fact]
    public void NumberDefs_定義が存在する()
    {
        Assert.NotEmpty(SegmentDefinitions.NumberDefs);
        Assert.Contains(SegmentDefinitions.NumberDefs, d => d.Label.Contains("数字"));
    }

    [Fact]
    public void AlphabetDefs_定義が存在する()
    {
        Assert.NotEmpty(SegmentDefinitions.AlphabetDefs);
        Assert.Contains(SegmentDefinitions.AlphabetDefs, d => d.Label.Contains("英字"));
    }

    [Fact]
    public void KanaDefs_3つの定義()
    {
        Assert.Equal(3, SegmentDefinitions.KanaDefs.Length);
    }

    [Fact]
    public void SymbolDefs_定義が存在する()
    {
        Assert.NotEmpty(SegmentDefinitions.SymbolDefs);
    }

    [Fact]
    public void EtcSpecialDefs_定義が存在する()
    {
        Assert.NotEmpty(SegmentDefinitions.EtcSpecialDefs);
    }

    [Fact]
    public void EtcBslashYenDefs_定義が存在する()
    {
        Assert.NotEmpty(SegmentDefinitions.EtcBslashYenDefs);
    }

    [Fact]
    public void EtcMultiSpaceDefs_定義が存在する()
    {
        Assert.NotEmpty(SegmentDefinitions.EtcMultiSpaceDefs);
    }

    [Fact]
    public void EtcZenHanAsciiDefs_定義が存在する()
    {
        Assert.NotEmpty(SegmentDefinitions.EtcZenHanAsciiDefs);
    }
}
