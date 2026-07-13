using System.Collections.Frozen;
using System.Collections.Generic;
using ClipboardZenHanConverter.App.Views;
using ClipboardZenHanConverter.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ClipboardZenHanConverter.App.Services;

/// <summary>アプリケーション内のページ遷移を管理します。</summary>
/// <remarks>ページの事前生成とキャッシュにより、遷移時の生成コストを削減します。<br/>
/// INavigationService インターフェースを実装し、DIP による依存性注入に対応します。<br/>
/// 最適化手法: <br/>
/// - _pageMap を FrozenDictionary で静的初期化（読み取り専用の高速ルックアップ）<br/>
/// - _pageCache でページインスタンスをキャッシュ（再生成コスト削減）</remarks>
public class NavigationService(IServiceProvider services) : INavigationService
{
    /// <summary>ページ名と型のマッピング。FrozenDictionary による高速・不変なルックアップ。</summary>
    private static readonly FrozenDictionary<string, Type> _pageMap = new Dictionary<string, Type>
    {
        ["Home"] = typeof(HomePage),
        ["Settings"] = typeof(SettingsPage),
    }.ToFrozenDictionary();

    /// <summary>キャッシュされたページインスタンス。初回アクセス時に生成され再利用されます。</summary>
    private readonly Dictionary<Type, UIElement> _pageCache = [];
    /// <summary>現在表示中のページタイプ。遷移時の差分判定に使用します。</summary>
    private Type? _currentPageType;

    /// <summary>ナビゲーションサービスを初期化します。</summary>
    public virtual void Initialize()
    {
        var dq = DispatcherQueue.GetForCurrentThread();
        dq?.TryEnqueue(DispatcherQueuePriority.Low,
            () => services.GetRequiredService(typeof(SettingsPage)));
    }

    /// <summary>指定されたページへ遷移します。</summary>
    public virtual void NavigateTo(object? selectedPage)
    {
        var tag = selectedPage switch
        {
            NavigationViewItem { Tag: string t } => t,
            NavigationViewItem item when ReferenceEquals(item, GetMainWindow().NavigationView.SettingsItem) => "Settings",
            string s => s,
            _ => null,
        };
        if (tag is null) return;

        NavigateToPage(tag);
    }

    /// <summary>DIコンテナからメインウィンドウインスタンスを取得します。</summary>
    /// <returns>メインウィンドウインスタンス</returns>
    private MainWindow GetMainWindow() => services.GetRequiredService<MainWindow>();

    /// <summary>指定されたタグ名のページへ遷移します。キャッシュがあれば再利用し、なければ生成してキャッシュに追加します。</summary>
    /// <param name="tag">遷移先ページのタグ名（"Home" または "Settings"）</param>
    private void NavigateToPage(string tag)
    {
        if (!_pageMap.TryGetValue(tag, out var pageType)) return;
        if (_currentPageType == pageType) return;

        var grid = GetMainWindow().ContentFrame;
        if (!_pageCache.TryGetValue(pageType, out var targetPage))
        {
            targetPage = (UIElement)services.GetRequiredService(pageType);
            _pageCache[pageType] = targetPage;
            grid.Children.Add(targetPage);
        }

        if (_currentPageType is not null && _pageCache.TryGetValue(_currentPageType, out var prev))
            prev.Visibility = Visibility.Collapsed;

        targetPage.Visibility = Visibility.Visible;
        _currentPageType = pageType;
    }
}

