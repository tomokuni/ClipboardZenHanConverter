using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace EsUtil.ClipboardZenHanConverter.Core.Native;

/// <summary>Win32 API からクリップボードを読み書きするための P/Invoke ラッパー。</summary>
/// <remarks>
/// NativeAOT 対応のため <c>LibraryImport</c> によるソース生成 P/Invoke を使用します。<br/>
/// UI フレームワークに依存しないため Core 層に配置します。<br/>
/// クリップボードの内容変更は <c>GetClipboardSequenceNumber</c> の値の変化で検出します。
/// </remarks>
public static partial class Win32Clipboard
{
    /// <summary>クリップボード形式 CF_UNICODETEXT（UTF-16 テキスト）。</summary>
    private const uint CfUnicodeText = 13;

    /// <summary>現在のクリップボードシーケンス番号を取得します。</summary>
    /// <returns>クリップボードが変更されるたびに増加するシーケンス番号。</returns>
    [LibraryImport("user32.dll")]
    public static partial uint GetClipboardSequenceNumber();

    /// <summary>指定されたテキストをクリップボードへ設定します。</summary>
    /// <param name="text">設定する UTF-16 文字列。</param>
    /// <exception cref="Win32Exception">クリップボード操作に失敗した場合。</exception>
    /// <remarks>アプリ終了後も内容を保持するため、<c>OpenClipboard</c> 後に内部で清掃されます。</remarks>
    public static void SetText(string text)
    {
        if (!OpenClipboard(IntPtr.Zero))
            throw new Win32Exception(Marshal.GetLastWin32Error());

        try
        {
            if (!EmptyClipboard())
                throw new Win32Exception(Marshal.GetLastWin32Error());

            var bytes = (text.Length + 1) * 2;
            var hGlobal = Marshal.AllocHGlobal(bytes);
            try
            {
                unsafe
                {
                    var ptr = (char*)hGlobal;
                    for (var i = 0; i < text.Length; i++) ptr[i] = text[i];
                    ptr[text.Length] = '\0';
                }

                if (SetClipboardData(CfUnicodeText, hGlobal) == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                hGlobal = IntPtr.Zero; // 所有権はクリップボードへ移る
            }
            finally
            {
                if (hGlobal != IntPtr.Zero) Marshal.FreeHGlobal(hGlobal);
            }
        }
        finally
        {
            CloseClipboard();
        }
    }

    /// <summary>クリップボードからテキストを取得します。テキストが無い場合は null を返します。</summary>
    /// <returns>取得したテキスト。テキスト形式でない場合は null。</returns>
    public static string? GetText()
    {
        if (!OpenClipboard(IntPtr.Zero))
            return null;

        try
        {
            var hGlobal = GetClipboardData(CfUnicodeText);
            if (hGlobal == IntPtr.Zero) return null;

            return Marshal.PtrToStringUni(hGlobal);
        }
        finally
        {
            CloseClipboard();
        }
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool OpenClipboard(IntPtr hWndNewOwner);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseClipboard();

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EmptyClipboard();

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetClipboardData(uint uFormat);
}
