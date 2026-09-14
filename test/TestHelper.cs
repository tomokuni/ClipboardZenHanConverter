using ClipboardZenHanConverter.Core.Models;

namespace ClipboardZenHanConverter.Tests;

/// <summary>テスト用の共通ヘルパーメソッドを提供します。</summary>
public static class TestHelper
{
    /// <summary>既定の変換設定を作成します。</summary>
    /// <returns>全モードが None の ConvertConfig インスタンス。</returns>
    /// <remarks>自動保存先は実ユーザーの設定ファイルを指すため、書き込みを伴うテストでは<br/>
    /// <see cref="SettingsPersistenceBase{TSettings}.AutoSaveFileName"/> を一時ファイルへ差し替えてください。</remarks>
    public static ConvertConfig CreateDefaultConfig() => new();

    /// <summary>テスト専用の一時ディレクトリを作成します。</summary>
    /// <returns>作成されたディレクトリのパス。</returns>
    public static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "ClipboardZenHanConverter.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>指定された条件が満たされるまで待機します。</summary>
    /// <param name="condition">待機終了条件。</param>
    /// <param name="timeoutMilliseconds">タイムアウト（ミリ秒）。</param>
    /// <returns>条件が満たされた場合は true。タイムアウトした場合は false。</returns>
    /// <remarks>非同期のイベントハンドラー（async void）の完了を待つために使用します。</remarks>
    public static async Task<bool> WaitUntilAsync(Func<bool> condition, int timeoutMilliseconds = 3000)
    {
        var elapsed = 0;
        while (!condition() && elapsed < timeoutMilliseconds)
        {
            await Task.Delay(10);
            elapsed += 10;
        }

        return condition();
    }
}
