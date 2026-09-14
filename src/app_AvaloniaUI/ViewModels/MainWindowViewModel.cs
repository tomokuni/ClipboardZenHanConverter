using Avalonia.Controls;
using EsUtil.ClipboardZenHanConverter.App.AvaloniaUI.Helpers;
using EsUtil.ClipboardZenHanConverter.App.AvaloniaUI.Services;
using EsUtil.ClipboardZenHanConverter.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using EsUtil.ClipboardZenHanConverter.Presentation.ViewModels;
using System.Collections.Generic;

namespace EsUtil.ClipboardZenHanConverter.App.AvaloniaUI.ViewModels;

/// <summary>メインウィンドウのデータを管理します。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - ナビゲーションペインの項目一覧と選択状態の保持<br/>
/// - 選択項目に対応する画面（<see cref="CurrentView"/>）の解決<br/>
/// - タイトルバーがバインドするアプリ設定・設定画面 ViewModel の公開（単一所有元への参照）<br/><br/>
/// 特徴: <br/>
/// - 画面解決は <see cref="INavigationService"/> に委譲し、ビューをキャッシュして再利用<br/>
/// - タイトルバーのクリップボード変換スイッチは <see cref="AppSetting.IsClipboardConvertEnabled"/> を
///   単一所有元とし、プリセット選択は <see cref="Settings"/>.SelectedPresetName を単一所有元とする
/// </remarks>
public partial class MainWindowViewModel : ObservableObject
{
    /// <summary>画面解決を行うナビゲーションサービス。</summary>
    private readonly INavigationService _navigation;

    /// <summary>アプリ設定を取得します。タイトルバーのクリップボード変換スイッチが参照します。</summary>
    public AppSetting AppSetting { get; }

    /// <summary>設定画面の ViewModel を取得します。タイトルバーのプリセット選択が参照します。</summary>
    public SettingsViewModel Settings { get; }

    /// <summary>ナビゲーションペインの項目一覧を取得します。</summary>
    public IReadOnlyList<NavigationItem> NavItems { get; }

    /// <summary>選択中のナビゲーション項目を取得または設定します。</summary>
    [ObservableProperty]
    public partial NavigationItem? SelectedNavItem { get; set; }

    /// <summary>現在表示中のビューを取得または設定します。</summary>
    [ObservableProperty]
    public partial Control? CurrentView { get; set; }

    /// <summary>MainWindowViewModel の新しいインスタンスを初期化します。</summary>
    /// <param name="appSetting">アプリ設定。</param>
    /// <param name="settings">設定画面の ViewModel。</param>
    /// <param name="navigation">画面解決を行うナビゲーションサービス。</param>
    public MainWindowViewModel(AppSetting appSetting, SettingsViewModel settings, INavigationService navigation)
    {
        AppSetting = appSetting;
        Settings = settings;
        _navigation = navigation;

        NavItems =
        [
            new NavigationItem("Home", "ホーム", FluentIcons.ConvertRange),
            new NavigationItem("Settings", "設定", FluentIcons.Settings),
        ];
    }

    /// <summary>選択項目の変更時に、対応する画面を解決して <see cref="CurrentView"/> へ設定します。</summary>
    /// <param name="value">新しく選択されたナビゲーション項目。</param>
    partial void OnSelectedNavItemChanged(NavigationItem? value)
    {
        CurrentView = value is null ? null : _navigation.ResolveView(value.Tag);
    }
}
