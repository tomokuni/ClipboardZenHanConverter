using EsUtil.ClipboardZenHanConverter.Core.Logic;
using Xunit;

namespace EsUtil.ClipboardZenHanConverter.Tests.Core.Logic;

/// <summary><see cref="ClipboardChangeDetector"/> の変更検出を検証します。</summary>
public sealed class ClipboardChangeDetectorTests
{
    /// <summary>検証対象が読み取るシーケンス番号（テストから書き換えます）。</summary>
    private uint _sequence;

    [Fact]
    public void HasChanged_構築直後の同じ値ではfalseを返す()
    {
        var detector = new ClipboardChangeDetector(() => _sequence);

        Assert.False(detector.HasChanged());
    }

    [Fact]
    public void HasChanged_値が変わるとtrueを返す()
    {
        var detector = new ClipboardChangeDetector(() => _sequence);

        _sequence = 10;

        Assert.True(detector.HasChanged());
    }

    [Fact]
    public void HasChanged_変化の検出は一度だけtrueを返す()
    {
        var detector = new ClipboardChangeDetector(() => _sequence);
        _sequence = 10;

        Assert.True(detector.HasChanged());
        Assert.False(detector.HasChanged());
    }

    [Fact]
    public void HasChanged_複数回の変化を順に検出する()
    {
        var detector = new ClipboardChangeDetector(() => _sequence);

        _sequence = 1;
        Assert.True(detector.HasChanged());

        _sequence = 2;
        Assert.True(detector.HasChanged());
        Assert.False(detector.HasChanged());

        _sequence = 3;
        Assert.True(detector.HasChanged());
    }

    [Fact]
    public void HasChanged_構築時の値を初期値として扱う()
    {
        _sequence = 42;

        var detector = new ClipboardChangeDetector(() => _sequence);

        Assert.False(detector.HasChanged());
    }
}
