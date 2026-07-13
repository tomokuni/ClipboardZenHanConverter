using System;
using System.Linq;
using ClipboardZenHanConverter.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ClipboardZenHanConverter.Views.Navigation;

/// <summary>アプリケーション内のページ遷移を管理するサービス実装です。</summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;
    private bool _isNavigating;

    /// <summary>ページ名とページ型のマッピング辞書です。</summary>
    private static readonly System.Collections.Generic.Dictionary<string, Type> _pageMap = new()
    {
        { "Home", typeof(HomePage) },
        { "Settings", typeof(SettingsPage) },
    };

    /// <summary>生成済みページインスタンスを保持するキャッシュです（Visual Tree から切り離さず常駐させる）。</summary>
    private readonly System.Collections.Generic.Dictionary<Type, UIElement> _pageCache = [];

    /// <summary>現在表示中のページタイプです。</summary>
    private Type? _currentPageType;

    /// <summary>現在表示中のページの INavigationAware インスタンスです。</summary>
    private INavigationAware? _currentPageAware;

    public NavigationService(IServiceProvider services)
    {
        _services = services;
    }

    /// <summary>初期表示ページに遷移します。</summary>
    public void Initialize()
    {
        // OnLaunched ですでに SelectedPage が設定されているため、
        // 改めて初期選択項目を探す必要はない（NavigateTo は再入ガードでスキップされる）

        // 設定画面の Singleton インスタンスを UI スレッドのアイドル時に事前生成する
        PreloadSettingsPageEagerly();
    }

    /// <summary>設定ページを UI スレッドのアイドル時に事前生成します。</summary>
    private void PreloadSettingsPageEagerly()
    {
        var dq = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        if (dq is null) return;

        dq.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
        {
            // Singleton の SettingsPage を DI 解決 → コンストラクタ + InitializeComponent() が実行される
            _ = _services.GetRequiredService(typeof(SettingsPage));
        });
    }

    /// <summary>指定されたページへ遷移します。</summary>
    /// <param name="selectedPage">遷移対象のページ情報（NavigationViewItem または型名文字列）</param>
    public void NavigateTo(object? selectedPage)
    {
        if (_isNavigating) return;
        _isNavigating = true;

        try
        {
            var mainWindow = GetMainWindow();
            var navigationView = mainWindow.NavigationView;

            var tag = ResolveTag(selectedPage, navigationView);
            if (tag is null) return;

            UpdateSelectedItem(tag, navigationView);
            NavigateToPage(tag, mainWindow);
        }
        finally
        {
            _isNavigating = false;
        }
    }

    private MainWindow GetMainWindow()
        => _services.GetRequiredService<MainWindow>();

    private static string? ResolveTag(object? selectedPage, NavigationView navigationView)
    {
        if (selectedPage is NavigationViewItem navItem)
        {
            var tag = navItem.Tag as string;
            if (tag is null && ReferenceEquals(navItem, navigationView.SettingsItem))
            {
                return "Settings";
            }
            return tag;
        }

        return selectedPage as string;
    }

    private static void UpdateSelectedItem(string tag, NavigationView navigationView)
    {
        if (tag == "Settings")
        {
            if (navigationView.SelectedItem != navigationView.SettingsItem)
                navigationView.SelectedItem = navigationView.SettingsItem;
            return;
        }

        var item = navigationView.MenuItems
            .OfType<NavigationViewItem>()
            .FirstOrDefault(i => i.Tag?.ToString() == tag);

        if (item is not null && !ReferenceEquals(navigationView.SelectedItem, item))
        {
            navigationView.SelectedItem = item;
        }
    }

    private void NavigateToPage(string tag, MainWindow mainWindow)
    {
        if (!_pageMap.TryGetValue(tag, out var pageType))
        {
            mainWindow.ContentFrame.Children.Clear();
            return;
        }

        // 同じページタイプなら遷移しない
        if (_currentPageType == pageType) return;

        var grid = mainWindow.ContentFrame;

        // 現在のページを非表示にしてライフサイクル終了を通知
        _currentPageAware?.OnNavigatingFrom();

        // 目的のページをキャッシュから取得、なければ DI で生成して Grid に追加
        if (!_pageCache.TryGetValue(pageType, out var targetPage))
        {
            targetPage = (UIElement)_services.GetRequiredService(pageType);
            _pageCache[pageType] = targetPage;
            grid.Children.Add(targetPage);
        }

        targetPage.Visibility = Visibility.Visible;

        // 以前のページを非表示にする（_currentPageType が null の初回はスキップ）
        if (_currentPageType is not null && _pageCache.TryGetValue(_currentPageType, out var prevPage))
        {
            prevPage.Visibility = Visibility.Collapsed;
        }

        // トラッキングを更新
        _currentPageType = pageType;
        _currentPageAware = targetPage as INavigationAware;

        // 新しいページのライフサイクル開始を通知
        _currentPageAware?.OnNavigatedTo(null);
    }
}

