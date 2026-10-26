using EsUtil.ZenHanConverter.App.WinForms.Services;
using EsUtil.ZenHanConverter.App.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using EsUtil.ZenHanConverter.App.Core.Presentation.ViewModels;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EsUtil.ZenHanConverter.App.WinForms.ViewModels;

/// <summary>メインウィンドウのデータを管理します。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - ナビゲーションペインの項目一覧と選択状態の保持<br/>
/// - 選択項目に対応する画面（<see cref="CurrentView"/>）の解決<br/>
/// - タイトルバーが参照するアプリ設定・設定画面 ViewModel の公開（単一所有元への参照）<br/><br/>
/// 特徴: <br/>
/// - 画面解決は <see cref="INavigationService"/> に委譲し、ビューをキャッシュして再利用<br/>
/// - タイトルバーのクリップボード変換スイッチは <see cref="AppSetting.IsClipboardConvertEnabled"/> を
///   単一所有元とし、プリセット選択は <see cref="Settings"/>.SelectedPresetName を単一所有元とする
/// </remarks>
/// <remarks>MainWindowViewModel の新しいインスタンスを初期化します。</remarks>
/// <param name="appSetting">アプリ設定。</param>
/// <param name="settings">設定画面の ViewModel。</param>
/// <param name="navigation">画面解決を行うナビゲーションサービス。</param>
public partial class MainWindowViewModel(AppSetting appSetting, SettingsViewModel settings, INavigationService navigation) : ObservableObject
{
    /// <summary>画面解決を行うナビゲーションサービス。</summary>
    private readonly INavigationService _navigation = navigation;

    /// <summary>アプリ設定を取得します。タイトルバーのクリップボード変換スイッチが参照します。</summary>
    public AppSetting AppSetting { get; } = appSetting;

    /// <summary>設定画面の ViewModel を取得します。タイトルバーのプリセット選択が参照します。</summary>
    public SettingsViewModel Settings { get; } = settings;

    /// <summary>ナビゲーションペインの項目一覧を取得します。</summary>
    public IReadOnlyList<NavigationItem> NavItems { get; } =
        [
            new NavigationItem("Home", "ホーム", NavigationIcon.ConvertRange),
            new NavigationItem("Settings", "設定", NavigationIcon.Settings),
        ];

    /// <summary>選択中のナビゲーション項目を取得または設定します。</summary>
    [ObservableProperty]
    public partial NavigationItem? SelectedNavItem { get; set; }

    /// <summary>現在表示中のビューを取得または設定します。</summary>
    [ObservableProperty]
    public partial Control? CurrentView { get; set; }

    /// <summary>選択項目の変更時に、対応する画面を解決して <see cref="CurrentView"/> へ設定します。</summary>
    /// <param name="value">新しく選択されたナビゲーション項目。</param>
    partial void OnSelectedNavItemChanged(NavigationItem? value)
    {
        CurrentView = value is null ? null : _navigation.ResolveView(value.Tag);
    }
}
