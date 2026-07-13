using ClipboardZenHanConverter.Core.Models;

namespace ClipboardZenHanConverter.Tests;

/// <summary>テスト用の共通ヘルパーメソッドを提供します。</summary>
public static class TestHelper
{
    /// <summary>全角/半角変換が有効なデフォルト設定を作成します。</summary>
    /// <returns>IsEnabledZenHan = true の ConvertConfig インスタンス</returns>
    public static ConvertConfig CreateDefaultConfig() => new() { IsEnabledZenHan = true };
}
