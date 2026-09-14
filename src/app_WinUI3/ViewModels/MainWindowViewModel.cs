using EsUtil.ClipboardZenHanConverter.App.WinUI.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace EsUtil.ClipboardZenHanConverter.App.WinUI.ViewModels;

/// <summary>メインウィンドウのデータを管理し、ページ間遷移を制御します。</summary>
/// <remarks>SelectedPage プロパティの変更時に自動的に INavigationService.NavigateTo を呼び出します。</remarks>
/// <param name="navigation">ページ遷移に使用するナビゲーションサービス。</param>
public partial class MainWindowViewModel(INavigationService navigation) : ObservableObject
{
    /// <summary>現在選択されているページです。</summary>
    /// <remarks>
    /// このプロパティの変更時に OnSelectedPageChanged が呼び出され、遷移処理が行われます。<br/>
    /// </remarks>
    [ObservableProperty]
    public partial object? SelectedPage { get; set; }

    /// <summary>SelectedPage が変更された時に呼び出され、ナビゲーションを実行します。</summary>
    /// <param name="value">新しく選択されたページ</param>
    partial void OnSelectedPageChanged(object? value)
    {
        // ナビゲーションサービスを使用してページ遷移を実行
        navigation.NavigateTo(value);
    }
}
