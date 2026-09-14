using Aprillz.MewUI.Controls;

namespace ClipboardZenHanConverter.App.MewUI.Services;

/// <summary>アプリケーション内のページ遷移を管理するインターフェース。</summary>
/// <remarks>NavigationView のコンテンツ解決（タグからビュー生成・キャッシュ）を抽象化します。<br/>
/// UI 要素（Element）を返すことから App 層に配置し、Core 層の UI 非依存を維持します。</remarks>
public interface INavigationService
{
    /// <summary>タグに対応するビューを解決します。</summary>
    /// <param name="tag">ページタグ（"Home" / "Settings"）。</param>
    /// <returns>タグに対応するビュー要素。</returns>
    Element ResolveView(string tag);
}
