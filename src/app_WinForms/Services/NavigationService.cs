using ClipboardZenHanConverter.App.WinForms.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Frozen;

namespace ClipboardZenHanConverter.App.WinForms.Services;

/// <summary>アプリケーション内の画面遷移を管理します。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - タグからビューを生成し、キャッシュして再利用<br/><br/>
/// 特徴: <br/>
/// - INavigationService インターフェースを実装し、DIP による依存性注入に対応<br/>
/// - タグとビュー型のマッピングは FrozenDictionary による不変・高速なルックアップ<br/>
/// - ビューは DI コンテナ経由で生成し、同一インスタンスを再利用する
/// </remarks>
public sealed class NavigationService(IServiceProvider services) : INavigationService
{
    /// <summary>タグ名とビュー型のマッピング。FrozenDictionary による高速・不変なルックアップ。</summary>
    private static readonly FrozenDictionary<string, Type> s_viewMap = new Dictionary<string, Type>
    {
        ["Home"] = typeof(HomeView),
        ["Settings"] = typeof(SettingsView),
    }.ToFrozenDictionary();

    /// <summary>キャッシュされたビューインスタンス。初回アクセス時に生成され再利用されます。</summary>
    private readonly Dictionary<string, Control> _viewCache = [];

    /// <summary>タグに対応するビューを生成（またはキャッシュから取得）します。</summary>
    /// <param name="tag">ページタグ（"Home" / "Settings"）。</param>
    /// <returns>タグに対応するビュー。</returns>
    /// <exception cref="InvalidOperationException">未対応のタグが指定された場合。</exception>
    public Control ResolveView(string tag)
    {
        if (_viewCache.TryGetValue(tag, out var cached))
            return cached;

        if (!s_viewMap.TryGetValue(tag, out var viewType))
            throw new InvalidOperationException($"Unknown navigation tag: {tag}");

        var view = (Control)services.GetRequiredService(viewType);
        _viewCache[tag] = view;
        return view;
    }
}
