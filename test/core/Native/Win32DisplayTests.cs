using ClipboardZenHanConverter.Core.Native;
using Xunit;

namespace ClipboardZenHanConverter.Tests.Core.Native;

/// <summary><see cref="Win32Display"/> のシステムメトリック取得を検証します。</summary>
public sealed class Win32DisplayTests
{
    [Fact]
    public void GetVirtualScreenBounds_正の幅と高さを返す()
    {
        var bounds = Win32Display.GetVirtualScreenBounds();

        Assert.True(bounds.Width > 0);
        Assert.True(bounds.Height > 0);
    }
}
