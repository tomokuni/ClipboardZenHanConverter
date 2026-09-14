using EsUtil.ClipboardZenHanConverter.App.WinUI.ViewModels;
using EsUtil.ClipboardZenHanConverter.Core.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace EsUtil.ClipboardZenHanConverter.App.WinUI.Views;

/// <summary>ホーム画面を表示するページクラスです。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - 変換前/変換後のテキスト表示<br/>
/// - 上下 2 分割と、分割バーのドラッグによる比率変更<br/><br/>
/// 特徴: <br/>
/// - プリセット選択と設定画面への遷移はタイトルバー / NavigationView が提供します<br/>
/// - 分割比率は行の重み（スターサイズ）で保持するため、ウィンドウの高さが変わっても比率が維持されます
/// </remarks>
public sealed partial class HomePage : Page
{
    /// <summary>現在の分割比率（上が占める割合）。</summary>
    private double _splitRatio = SplitLayout.DefaultRatio;

    /// <summary>ホームページ用のViewModelです。</summary>
    public HomeViewModel ViewModel { get; }

    /// <summary>HomePage の新しいインスタンスを初期化します。</summary>
    /// <param name="viewModel">ホームページ用の ViewModel</param>
    public HomePage(HomeViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();

        SplitterRegion.DeltaChanged += OnSplitterDeltaChanged;
        ApplyRatio(_splitRatio);
    }

    /// <summary>分割バーのドラッグ中に分割比率を更新します。</summary>
    /// <param name="verticalChange">前回の通知からの垂直方向の移動量。</param>
    private void OnSplitterDeltaChanged(double verticalChange)
    {
        var trackHeight = SplitGrid.ActualHeight - SplitterRegion.ActualHeight;
        ApplyRatio(SplitLayout.RatioFromDrag(_splitRatio, verticalChange, trackHeight));
    }

    /// <summary>分割比率を上下の行の重みへ反映します。</summary>
    /// <param name="ratio">適用する分割比率。</param>
    /// <remarks>重みで保持するため、ウィンドウの高さが変化しても比率が維持されます。</remarks>
    private void ApplyRatio(double ratio)
    {
        _splitRatio = SplitLayout.ClampRatio(ratio);
        TopRow.Height = new GridLength(SplitLayout.TopWeight(_splitRatio), GridUnitType.Star);
        BottomRow.Height = new GridLength(SplitLayout.BottomWeight(_splitRatio), GridUnitType.Star);
    }
}
