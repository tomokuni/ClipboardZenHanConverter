using Avalonia.Controls;
using Avalonia.Threading;
using EsUtil.ClipboardZenHanConverter.App.AvaloniaUI.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace EsUtil.ClipboardZenHanConverter.App.AvaloniaUI.Services;

/// <summary>アプリケーション内の画面遷移を管理します。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - タグからビューを生成し、キャッシュして再利用<br/>
/// - バックグラウンドでのビュー事前生成（UI スレッドがアイドル状態のタイミング）<br/><br/>
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

    /// <summary>指定されたタグのビューを UI スレッドのアイドル時に事前生成します。</summary>
    /// <param name="tag">ページタグ（"Home" / "Settings"）。</param>
    /// <remarks>生成とキャッシュのみを行い、表示は遷移時に行います（レイアウトを汚さないため）。</remarks>
    public void PreloadView(string tag)
    {
        if (_viewCache.ContainsKey(tag))
            return;

        Dispatcher.UIThread.Post(() =>
        {
            if (_viewCache.ContainsKey(tag))
                return;

            ResolveView(tag);
        }, DispatcherPriority.Background);
    }
}
