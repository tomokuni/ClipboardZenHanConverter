using Aprillz.MewUI.Controls;
using ClipboardZenHanConverter.App.MewUI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ClipboardZenHanConverter.App.MewUI.Services;

/// <summary>アプリケーション内のページ遷移を管理します。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - NavigationView のコンテンツ解決（タグからビューを生成・キャッシュ）<br/>
/// - HomeView / SettingsView のキャッシュによる再生成コスト削減<br/><br/>
/// 特徴: <br/>
/// - INavigationService インターフェースを実装し、DIP による依存性注入に対応<br/>
/// - ビュー解決は NavigationView.ContentSelector から ResolveView 経由で行う<br/>
/// - キャッシュにより遷移時の生成コストを削減
/// </remarks>
public sealed class NavigationService(IServiceProvider services) : INavigationService
{
    /// <summary>キャッシュされたビューインスタンス。初回アクセス時に生成され再利用されます。</summary>
    private readonly Dictionary<string, Element> _viewCache = [];

    /// <summary>タグに対応するビューを生成（またはキャッシュから取得）します。</summary>
    /// <param name="tag">ページタグ（"Home" / "Settings"）。</param>
    /// <returns>タグに対応するビュー要素。</returns>
    /// <exception cref="InvalidOperationException">未対応のタグが指定された場合。</exception>
    /// <remarks>NavigationView.ContentSelector から呼び出され、ビューを再利用可能にキャッシュします。</remarks>
    public Element ResolveView(string tag)
    {
        if (_viewCache.TryGetValue(tag, out var cached))
            return cached;

        var view = CreateView(tag);
        _viewCache[tag] = view;
        return view;
    }

    /// <summary>タグ名からビューインスタンスを生成します。</summary>
    /// <param name="tag">ページタグ（"Home" / "Settings"）。</param>
    /// <returns>生成されたビュー。</returns>
    private Element CreateView(string tag) => tag switch
    {
        "Home" => services.GetRequiredService<HomeView>(),
        "Settings" => services.GetRequiredService<SettingsView>(),
        _ => throw new InvalidOperationException($"Unknown navigation tag: {tag}"),
    };
}
