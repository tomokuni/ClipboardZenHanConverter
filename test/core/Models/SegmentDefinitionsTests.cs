using EsUtil.ClipboardZenHanConverter.Core.Enums;
using EsUtil.ClipboardZenHanConverter.Core.Models;
using System;
using System.Linq;
using Xunit;

namespace EsUtil.ClipboardZenHanConverter.Tests.Core.Models;

/// <summary><see cref="SegmentDefinitions"/> の定義内容を検証します。</summary>
public sealed class SegmentDefinitionsTests
{
    /// <summary>全カテゴリの定義配列。</summary>
    private static readonly SegmentDefine[][] CategoryDefinitions =
    [
        SegmentDefinitions.NumberDefs,
        SegmentDefinitions.AlphabetDefs,
        SegmentDefinitions.KanaDefs,
        SegmentDefinitions.SymbolDefs,
        SegmentDefinitions.EtcZenHanAsciiDefs,
        SegmentDefinitions.EtcBslashYenDefs,
        SegmentDefinitions.EtcSpecialDefs,
        SegmentDefinitions.EtcMultiSpaceDefs,
    ];

    /// <summary>全カテゴリの定義を平坦化した配列。</summary>
    private static readonly SegmentDefine[] AllDefinitions = [.. CategoryDefinitions.SelectMany(d => d)];

    /// <summary>全カテゴリの定義配列をテストデータとして提供します。</summary>
    /// <returns>カテゴリ名と定義配列の組。</returns>
    public static TheoryData<string, SegmentDefine[]> CategoryTheoryData()
    {
        var data = new TheoryData<string, SegmentDefine[]>();
        foreach (var defs in CategoryDefinitions)
        {
            data.Add(defs[0].Label, defs);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(CategoryTheoryData))]
    public void 各カテゴリの定義は空でなくラベルを持つ(string categoryLabel, SegmentDefine[] defs)
    {
        Assert.False(string.IsNullOrEmpty(categoryLabel));
        Assert.NotEmpty(defs);
        Assert.All(defs, d => Assert.False(string.IsNullOrWhiteSpace(d.Label)));
    }

    [Fact]
    public void 全カテゴリでMode定義が設定され一意である()
    {
        Assert.All(AllDefinitions, d => Assert.NotNull(d.Mode));
        Assert.Equal(AllDefinitions.Length, AllDefinitions.Select(d => d.Mode).Distinct().Count());
    }

    [Fact]
    public void 数字と英字と連続スペースは単一項目()
    {
        Assert.Single(SegmentDefinitions.NumberDefs);
        Assert.Single(SegmentDefinitions.AlphabetDefs);
        Assert.Single(SegmentDefinitions.EtcMultiSpaceDefs);
    }

    [Fact]
    public void かなは3項目で各4セグメントを持つ()
    {
        Assert.Equal(3, SegmentDefinitions.KanaDefs.Length);
        Assert.All(SegmentDefinitions.KanaDefs, d => Assert.Equal(4, d.Segments?.Length));
    }

    [Fact]
    public void 記号は34項目()
    {
        Assert.Equal(34, SegmentDefinitions.SymbolDefs.Length);
    }

    [Fact]
    public void 記号のラベルは名称と記号の2部構成()
    {
        Assert.All(SegmentDefinitions.SymbolDefs, d => Assert.Contains('\u3000', d.Label));
    }

    [Fact]
    public void 特殊文字はタブと改行の2項目()
    {
        Assert.Equal(2, SegmentDefinitions.EtcSpecialDefs.Length);
    }

    [Fact]
    public void バックスラッシュと円記号は4項目()
    {
        Assert.Equal(4, SegmentDefinitions.EtcBslashYenDefs.Length);
    }

    [Fact]
    public void 連続スペースは高さを指定する()
    {
        Assert.Equal(80, SegmentDefinitions.EtcMultiSpaceDefs[0].Height);
    }

    [Fact]
    public void セグメントを持つ定義はModeで設定を往復できる()
    {
        var config = TestHelper.CreateDefaultConfig();
        var defs = AllDefinitions.Where(d => d.Segments is { Length: > 0 });

        foreach (var def in defs)
        {
            var value = def.Segments!.First(s => s.IsEnabled).Value;
            def.Mode.Set(config, value);
            Assert.Equal(value, def.Mode.Get(config));
        }
    }

    [Fact]
    public void かな定義は対応するモードに接続されている()
    {
        var config = TestHelper.CreateDefaultConfig();

        SegmentDefinitions.KanaDefs[0].Mode.Set(config, ZenHanKanaMode.ToZenKata);

        Assert.Equal(ZenHanKanaMode.ToZenKata, config.ConvertModeKanaHan);
    }

    [Fact]
    public void 記号定義は記号モードに接続されている()
    {
        var config = TestHelper.CreateDefaultConfig();

        SegmentDefinitions.SymbolDefs[0].Mode.Set(config, ZenHanMode.ToHan);

        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeSymbolParenthesis);
    }
}
