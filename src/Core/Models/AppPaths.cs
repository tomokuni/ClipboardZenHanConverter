using System;
using System.IO;

namespace EsUtil.ZenHanConverter.Core.Models;

/// <summary>アプリが使うフォルダーの場所を単一所有します。</summary>
/// <remarks>保存先のルートは <c>%LOCALAPPDATA%\ZenHanConverter</c> です。<br/>
/// 旧名（<c>ClipboardZenHanConverter</c>）のフォルダーは、<see cref="Migrate"/> が初回だけ内容を写します。</remarks>
public static class AppPaths
{
    /// <summary>現在の製品名（保存先フォルダー名にも使う）。</summary>
    public const string ProductName = "ZenHanConverter";

    /// <summary>旧製品名（移行元のフォルダー名）。</summary>
    private const string LegacyProductName = "ClipboardZenHanConverter";

    /// <summary>設定とプリセットを置くルート フォルダーを取得します。</summary>
    /// <value><c>%LOCALAPPDATA%\ZenHanConverter</c></value>
    public static string RootFolder { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        ProductName);

    /// <summary>旧名のルート フォルダーを取得します（移行元）。</summary>
    public static string LegacyRootFolder { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        LegacyProductName);

    /// <summary>ルート フォルダー配下のファイルのパスを組み立てます。</summary>
    /// <param name="fileName">ファイル名（サブ フォルダーを含めない）。</param>
    /// <returns>組み立てた絶対パス。</returns>
    public static string GetFilePath(string fileName) => Path.Combine(RootFolder, fileName);

    /// <summary>旧名のフォルダーから現在のフォルダーへ内容を移行します（初回のみ）。</summary>
    /// <returns>移行したファイル数。移行が不要だった場合と、移行に失敗した場合は 0。</returns>
    /// <remarks>新しいフォルダーにまだ何も無く、旧フォルダーがある場合だけ写します（上書きしません）。<br/>
    /// 旧フォルダーは残します（移行後に利用者が判断して削除できるようにするため）。<br/>
    /// **移行は起動を止めない best-effort** とします（読めないファイルがあっても、アプリは既定値で起動できます）。</remarks>
    public static int Migrate()
    {
        try
        {
            if (Directory.Exists(RootFolder) || !Directory.Exists(LegacyRootFolder))
            {
                return 0;
            }

            var migrated = 0;
            foreach (var source in Directory.GetFiles(LegacyRootFolder, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(LegacyRootFolder, source);
                var destination = Path.Combine(RootFolder, relative);
                var directory = Path.GetDirectoryName(destination);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.Copy(source, destination, overwrite: false);
                migrated++;
            }

            return migrated;
        }
        catch (IOException)
        {
            // 使われている・権限が無いなどで写せない場合は、そのまま既定値で起動する
            return 0;
        }
        catch (UnauthorizedAccessException)
        {
            // 同上（旧フォルダーへ入れない環境）
            return 0;
        }
    }
}
