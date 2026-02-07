using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace ClipboardZenHanConverter.Views.Navigation
{
    /// <summary>アプリケーション内のページ遷移を管理するサービス実装。</summary>
    /// <remarks>
    /// 初期表示ページの設定や、選択されたページへの遷移処理を提供します。<br/>
    /// <br/>
    /// 【特徴】<br/>
    /// - NavigationViewItem の Value プロパティからページ型名を取得し、DI コンテナからインスタンスを解決してフレームに表示します。<br/>
    /// <br/>
    /// 【実装の詳細】<br/>
    /// - リフレクションを使用してアセンブリからページ型を検索します。<br/>
    /// <br/>
    /// 【注意点】<br/>
    /// - ページ型名が一致しない場合や DI 登録がない場合は遷移しません。<br/>
    /// </remarks>
    public class NavigationService : INavigationService
    {
        /// <summary>初期表示ページを NavigationViewItem.IsSelected が true の項目に設定し、遷移します。</summary>
        /// <remarks>
        /// MainWindow の NavigationView から IsSelected の NavigationViewItem を取得し、NavigateTo メソッドで遷移します。<br/>
        /// <br/>
        /// 【処理フロー】<br/>
        /// - 選択項目の取得 → 遷移処理の呼び出しです。<br/>
        /// <br/>
        /// 【注意点】<br/>
        /// - 選択項目がない場合は遷移しません。<br/>
        /// </remarks>
        public void Initialize()
        {
            // MainWindow の NavigationView を取得
            var navigationView = App.GetService<MainWindow>().NavigationView;

            // NavigationView の MenuItems から IsSelected が true の NavigationViewItem を検索
            var selectedPage = navigationView.MenuItems
                .OfType<NavigationViewItem>()
                .FirstOrDefault(navItem => navItem.IsSelected);

            if (selectedPage is null)
            {
                // 何も選択されていない場合は設定を初期選択にする
                NavigateTo("Settings");
                return;
            }
            else
            {
                // 選択された NavigationViewItem を元にページ遷移を実行
                NavigateTo(selectedPage);
            }
        }

        /// <summary>指定されたページ情報に基づき、対応するページ型へ遷移します。</summary>
        /// <remarks>
        /// NavigationViewItem の場合は Value プロパティから型名を取得し、文字列の場合はそのまま扱います。<br/>
        /// <br/>
        /// 【処理フロー】<br/>
        /// - 型名の取得 → ページ型の検索 → DI からのインスタンス取得 → フレームへの設定です。<br/>
        /// <br/>
        /// 【注意点】<br/>
        /// - 型が見つからない場合や現在表示中と同じ場合は遷移しません。<br/>
        /// </remarks>
        /// <param name="selectedPage">遷移対象のページ情報（NavigationViewItem または型名文字列）</param>
        public void NavigateTo(object? selectedPage)
        {
            // MainWindow とその中の NavigationView を取得
            var mainWindow = App.GetService<MainWindow>();
            var navigationView = mainWindow.NavigationView;

            // 選択されたページ情報から型名を抽出
            string? tag = selectedPage switch
            {
                NavigationViewItem navItem => navItem.Tag as string, // NavigationViewItem の Value から型名を取得
                string str => str,                                   // 文字列の場合はそのまま使用
                _ => null                                            // その他の場合は null
            };

            // ナビゲーションメニュー項目から、指定されたタグに一致する項目を探して選択状態にする
            if (tag is not null)
            {
                // 通常のメニュー項目から検索
                var item = navigationView.MenuItems
                    .OfType<NavigationViewItem>()
                    .FirstOrDefault(i => i.Tag?.ToString() == tag);

                // 見つからない場合はフッターメニューからも検索
                item ??= navigationView.FooterMenuItems
                    .OfType<NavigationViewItem>()
                    .FirstOrDefault(i => i.Tag?.ToString() == tag);

                // 設定項目（特殊な項目）のチェック
                if (item == null && tag == "Settings")
                {
                    if (navigationView.SelectedItem != navigationView.SettingsItem)
                        navigationView.SelectedItem = navigationView.SettingsItem;
                }
                else if (item != null && navigationView.SelectedItem as NavigationViewItem != item)
                {
                    navigationView.SelectedItem = item;
                }
            }

            // MainWindow の ContentFrame と DI サービスプロバイダーを取得
            var frame = mainWindow.ContentFrame;
            var serviceProvider = App.Services;


            // 型名が存在し、対応するページ型が見つかり、現在表示中と異なる場合のみ遷移
            if (tag is not null
                && GetTypeByPartialForPage(tag) is var pageType
                && pageType is not null
                && frame.CurrentSourcePageType != pageType)
            {
                // DI コンテナからページインスタンスを取得し、フレームに設定
                var pageInstance = serviceProvider.GetRequiredService(pageType);
                frame.Content = pageInstance;
            }
            else
            {
                // 条件を満たさない場合は空の Frame を表示
                frame.Content = new Frame();
            }
        }

        /// <summary>
        /// ページ名（タグ）とページ型の対応を定義するマッピング辞書です。
        /// </summary>
        /// <remarks>
        /// AOT/トリミング環境でリフレクションによる型探索を回避するために使用します。<br/>
        /// </remarks>
        private static readonly System.Collections.Generic.Dictionary<string, Type> _pageMap = new()
        {
            { "Home", typeof(HomePage) },
            { "Settings", typeof(SettingsPage) },
            { "HomePage", typeof(HomePage) },
            { "SettingsPage", typeof(SettingsPage) }
        };

        /// <summary>指定されたタグ名に対応するページ型を取得します。</summary>
        /// <remarks>
        /// 辞書から一致する型を検索します。<br/>
        /// </remarks>
        /// <param name="partialName">型名またはタグ名</param>
        /// <returns>一致した型。見つからない場合は null</returns>
        private static Type? GetTypeByPartialForPage(string partialName)
        {
            if (_pageMap.TryGetValue(partialName, out var type))
            {
                return type;
            }

            return null;
        }
    }
}

