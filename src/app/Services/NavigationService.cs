using System.Collections.Generic;
using ClipboardZenHanConverter.Core.Interfaces;
using ClipboardZenHanConverter.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ClipboardZenHanConverter.Services;

/// <summary>アプリケーション内のページ遷移を管理するサービス実装です。</summary>
public class NavigationService(IServiceProvider services) : INavigationService
{
    private bool _isNavigating;

    private static readonly Dictionary<string, Type> _pageMap = new()
    {
        { "Home", typeof(HomePage) },
        { "Settings", typeof(SettingsPage) },
    };

    private readonly Dictionary<Type, UIElement> _pageCache = [];
    private Type? _currentPageType;
    private INavigationAware? _currentPageAware;

    public void Initialize()
    {
        // SettingsPage を UI スレッドのアイドル時に事前生成
        var dq = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        dq?.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
            () => services.GetRequiredService(typeof(SettingsPage)));
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
        => services.GetRequiredService<MainWindow>();

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

        if (_currentPageType == pageType) return;

        _currentPageAware?.OnNavigatingFrom();

        var grid = mainWindow.ContentFrame;
        if (!_pageCache.TryGetValue(pageType, out var targetPage))
        {
            targetPage = (UIElement)services.GetRequiredService(pageType);
            _pageCache[pageType] = targetPage;
            grid.Children.Add(targetPage);
        }

        targetPage.Visibility = Visibility.Visible;

        if (_currentPageType is not null && _pageCache.TryGetValue(_currentPageType, out var prevPage))
        {
            prevPage.Visibility = Visibility.Collapsed;
        }

        _currentPageType = pageType;
        _currentPageAware = targetPage as INavigationAware;
        _currentPageAware?.OnNavigatedTo(null);
    }
}

