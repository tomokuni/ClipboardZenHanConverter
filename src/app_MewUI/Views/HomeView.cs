using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using EsUtil.ClipboardZenHanConverter.App.MewUI.ViewModels;

namespace EsUtil.ClipboardZenHanConverter.App.MewUI.Views;

/// <summary>ホーム画面を表示するビューです。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - 変換前/変換後のテキスト表示<br/>
/// - 変換前テキストの変更に対する自動変換<br/>
/// - 上下 2 分割と、分割バーのドラッグによる比率変更<br/><br/>
/// 特徴: <br/>
/// - コードファースト（C# Markup）で構築<br/>
/// - ObservableObject とのバインディング<br/>
/// - プリセット選択・クリップボード変換スイッチはタイトルバー（MainWindow）が所有<br/>
/// - 変換前テキストボックスは常に編集可能。変更時に ViewModel の BeforeText へ伝わり変換されます<br/>
/// - 上下の分割は MewUI の <see cref="SplitPanel"/> に委譲します。長さをスターサイズで保持するため、
///   ウィンドウの高さが変わっても上下の比率が維持されます
/// </remarks>
public sealed class HomeView : UserControl
{
    /// <summary>分割バーの太さ（論理ピクセル）。</summary>
    private const double SplitterThickness = 6;

    /// <summary>画面の余白（論理ピクセル）。</summary>
    private const double ContentMargin = 24;

    /// <summary>ペインの最小高さ（論理ピクセル）。</summary>
    private const double MinPaneHeight = 40;

    /// <summary>テキスト領域の内側の余白（論理ピクセル）。</summary>
    private const double TextAreaPadding = 8;

    /// <summary>ホーム画面の ViewModel。</summary>
    private readonly HomeViewModel _viewModel;

    /// <summary>HomeView の新しいインスタンスを初期化します。</summary>
    /// <param name="viewModel">ホーム画面の ViewModel。</param>
    public HomeView(HomeViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    /// <summary>ビューの内容を構築します。</summary>
    /// <returns>ホーム画面のルート要素。</returns>
    protected override Element? OnBuild()
    {
        var beforeText = new ObservableValue<string>(_viewModel.BeforeText);
        var convertedText = new ObservableValue<string>(_viewModel.ConvertedText);

        // ViewModel の変更を ObservableValue に反映
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(HomeViewModel.BeforeText))
                beforeText.Value = _viewModel.BeforeText;
            else if (e.PropertyName == nameof(HomeViewModel.ConvertedText))
                convertedText.Value = _viewModel.ConvertedText;
        };

        return new SplitPanel()
            .Vertical()
            .SplitterThickness(SplitterThickness)
            .FirstLength(GridLength.Star)
            .SecondLength(GridLength.Star)
            .MinFirst(MinPaneHeight)
            .MinSecond(MinPaneHeight)
            .Margin(ContentMargin)
            .First(BuildBeforePane(beforeText))
            .Second(BuildAfterPane(convertedText));
    }

    /// <summary>変換前のペイン（見出し + 編集可能なテキスト）を構築します。</summary>
    /// <param name="beforeText">変換前テキストのバインド元。</param>
    /// <returns>変換前のペイン。</returns>
    /// <remarks>変換前テキストは編集可能。変更されると ViewModel 経由で自動変換されます。</remarks>
    private UIElement BuildBeforePane(ObservableValue<string> beforeText)
        => BuildPane(
            "変更前：",
            new MultiLineTextBox()
                .BindText(beforeText)
                .OnTextChanged(value => _viewModel.BeforeText = value));

    /// <summary>変換後のペイン（見出し + 読み取り専用のテキスト）を構築します。</summary>
    /// <param name="convertedText">変換後テキストのバインド元。</param>
    /// <returns>変換後のペイン。</returns>
    private UIElement BuildAfterPane(ObservableValue<string> convertedText)
        => BuildPane(
            "変更後：",
            new MultiLineTextBox()
                .BindText(convertedText)
                .IsReadOnly(true));

    /// <summary>見出しとテキスト領域を縦に並べたペインを構築します。</summary>
    /// <param name="caption">見出しの文言。</param>
    /// <param name="textBox">表示するテキストボックス。</param>
    /// <returns>構築したペイン。</returns>
    /// <remarks>テキスト領域は残りの高さをすべて使用します（行の重みをスターサイズで指定）。</remarks>
    private static UIElement BuildPane(string caption, UIElement textBox)
        => new Grid()
            .Rows("Auto,*")
            .Children(
                new Label()
                    .Text(caption)
                    .Bold()
                    .Margin(0, 0, 0, 4)
                    .Row(0),
                new Border()
                    .Padding(TextAreaPadding)
                    .Row(1)
                    .Child(textBox));
}
