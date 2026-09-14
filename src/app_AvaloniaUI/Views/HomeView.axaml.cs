using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using ClipboardZenHanConverter.App.AvaloniaUI.ViewModels;
using ClipboardZenHanConverter.Core.Helpers;

namespace ClipboardZenHanConverter.App.AvaloniaUI.Views;

/// <summary>ホーム画面を表示するビューです。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - 変換前/変換後のテキスト表示<br/>
/// - 変換前テキストの編集（変更時に ViewModel 経由で自動変換）<br/>
/// - 上下 2 分割と、分割バーのドラッグによる比率変更<br/><br/>
/// 特徴: <br/>
/// - クリップボード変換スイッチとプリセット選択はタイトルバー（MainWindow）が所有<br/>
/// - 全角/半角変換は常に有効のため、変換の有効/無効を切り替える UI は持ちません<br/>
/// - 分割比率は行の重み（スターサイズ）で保持するため、ウィンドウの高さが変わっても比率が維持されます
/// </remarks>
public partial class HomeView : UserControl
{
    /// <summary>現在の分割比率（上が占める割合）。</summary>
    private double _splitRatio = SplitLayout.DefaultRatio;

    /// <summary>ホーム画面の ViewModel を取得します。</summary>
    public HomeViewModel ViewModel { get; }

    /// <summary>HomeView の新しいインスタンスを初期化します。</summary>
    /// <param name="viewModel">ホーム画面の ViewModel。</param>
    public HomeView(HomeViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;

        ApplyRatio(_splitRatio);
    }

    /// <summary>分割バーのドラッグ中に分割比率を更新します。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">ドラッグ量を含むイベントデータ。</param>
    /// <remarks><see cref="Thumb"/> の <c>DragDelta</c> は前回からの差分を通知するため、
    /// 現在の比率へ差分を加算していきます。</remarks>
    private void OnSplitterDragDelta(object? sender, VectorEventArgs e)
    {
        var trackHeight = SplitGrid.Bounds.Height - Splitter.Bounds.Height;
        ApplyRatio(SplitLayout.RatioFromDrag(_splitRatio, e.Vector.Y, trackHeight));
    }

    /// <summary>分割比率を上下の行の重みへ反映します。</summary>
    /// <param name="ratio">適用する分割比率。</param>
    /// <remarks>重みで保持するため、ウィンドウの高さが変化しても比率が維持されます。</remarks>
    private void ApplyRatio(double ratio)
    {
        _splitRatio = SplitLayout.ClampRatio(ratio);
        SplitGrid.RowDefinitions[0].Height = new GridLength(SplitLayout.TopWeight(_splitRatio), GridUnitType.Star);
        SplitGrid.RowDefinitions[2].Height = new GridLength(SplitLayout.BottomWeight(_splitRatio), GridUnitType.Star);
    }
}
