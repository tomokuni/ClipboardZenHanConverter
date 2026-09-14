using EsUtil.ClipboardZenHanConverter.Core.Native;
using Xunit;

namespace EsUtil.ClipboardZenHanConverter.Tests.Core.Native;

/// <summary><see cref="Win32Clipboard"/> の読み取り系 API を検証します。</summary>
/// <remarks>クリップボードを書き換える <c>SetText</c> は、実ユーザーのクリップボード内容を<br/>
/// 破壊するため検証しません。</remarks>
public sealed class Win32ClipboardTests
{
    [Fact]
    public void GetClipboardSequenceNumber_例外を投げずに取得できる()
    {
        var exception = Record.Exception(() => Win32Clipboard.GetClipboardSequenceNumber());

        Assert.Null(exception);
    }
}
