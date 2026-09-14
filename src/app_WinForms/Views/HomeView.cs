using EsUtil.ClipboardZenHanConverter.App.WinForms.ViewModels;
using EsUtil.ClipboardZenHanConverter.Core.Helpers;
using System.Drawing;
using System.Windows.Forms;

namespace EsUtil.ClipboardZenHanConverter.App.WinForms.Views;

/// <summary>ホーム画面を表示するビューです。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - 変換前/変換後のテキスト表示<br/>
/// - 変換前テキストの編集（変更時に ViewModel 経由で自動変換）<br/>
/// - 上下 2 分割と、分割バーのドラッグによる比率変更<br/><br/>
/// 特徴: <br/>
/// - クリップボード変換スイッチとプリセット選択はタイトルバー（MainForm）が所有<br/>
/// - 全角/半角変換は常に有効のため、変換の有効/無効を切り替える UI は持ちません<br/>
/// - 変換前は双方向（編集の即時反映）、変換後は読み取り専用のバインドとします<br/>
/// - 分割比率は上下の行を割合（Percent）で保持するため、ウィンドウの高さが変わっても比率が維持されます
/// </remarks>
public sealed class HomeView : UserControl
{
    /// <summary>画面の余白（論理ピクセル）。</summary>
    private static readonly Padding ContentPadding = new(40, 20, 40, 40);

    /// <summary>分割バーの高さ（論理ピクセル）。</summary>
    private const int SplitterHeight = 6;

    /// <summary>見出しとテキストボックスの間隔（論理ピクセル）。</summary>
    private const int CaptionSpacing = 2;

    /// <summary>分割バー。</summary>
    private readonly Panel _splitter;

    /// <summary>上下分割のレイアウト。</summary>
    private readonly TableLayoutPanel _layout;

    /// <summary>現在の分割比率（上が占める割合）。</summary>
    private double _splitRatio = SplitLayout.DefaultRatio;

    /// <summary>分割バーをドラッグ中かどうか。</summary>
    private bool _isDraggingSplitter;

    /// <summary>ドラッグ開始時のマウス Y 座標（レイアウト座標）。</summary>
    private int _splitterDragStartY;

    /// <summary>ドラッグ開始時の分割比率。</summary>
    private double _splitterDragStartRatio;

    /// <summary>ホーム画面の ViewModel を取得します。</summary>
    public HomeViewModel ViewModel { get; }

    /// <summary>HomeView の新しいインスタンスを初期化します。</summary>
    /// <param name="viewModel">ホーム画面の ViewModel。</param>
    public HomeView(HomeViewModel viewModel)
    {
        ViewModel = viewModel;
        Dock = DockStyle.Fill;
        Padding = ContentPadding;

        _splitter = CreateSplitter();

        _layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, SplitterHeight));
        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

        _layout.Controls.Add(CreatePane("変更前：", CreateTextBox(isEditable: true)), 0, 0);
        _layout.Controls.Add(_splitter, 0, 1);
        _layout.Controls.Add(CreatePane("変更後：", CreateTextBox(isEditable: false)), 0, 2);

        Controls.Add(_layout);
        ApplyRatio(_splitRatio);
    }

    /// <summary>分割比率を上下の行へ反映します。</summary>
    /// <param name="ratio">適用する分割比率。</param>
    /// <remarks>割合で保持するため、ウィンドウの高さが変化しても比率が維持されます。</remarks>
    private void ApplyRatio(double ratio)
    {
        _splitRatio = SplitLayout.ClampRatio(ratio);
        _layout.RowStyles[0].Height = (float)(SplitLayout.TopWeight(_splitRatio) * 100);
        _layout.RowStyles[2].Height = (float)(SplitLayout.BottomWeight(_splitRatio) * 100);
    }

    /// <summary>分割バーを作成します。</summary>
    /// <returns>作成した分割バー。</returns>
    private Panel CreateSplitter()
    {
        var splitter = new Panel
        {
            Dock = DockStyle.Fill,
            Cursor = Cursors.SizeNS,
            Margin = new Padding(0),
        };

        splitter.Paint += (_, e) =>
        {
            // 区切り線のみを描き、残りの領域はドラッグの当たり判定に使用する
            var y = splitter.ClientSize.Height / 2;
            using var pen = new Pen(AppTheme.BorderColor);
            e.Graphics.DrawLine(pen, 0, y, splitter.ClientSize.Width, y);
        };

        splitter.MouseDown += OnSplitterMouseDown;
        splitter.MouseMove += OnSplitterMouseMove;
        splitter.MouseUp += OnSplitterMouseUp;
        return splitter;
    }

    /// <summary>分割バーでマウスが押されたときにドラッグを開始します。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">マウスイベントデータ。</param>
    private void OnSplitterMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;

        _isDraggingSplitter = true;
        _splitterDragStartY = _layout.PointToClient(Cursor.Position).Y;
        _splitterDragStartRatio = _splitRatio;
        _splitter.Capture = true;
    }

    /// <summary>分割バーのドラッグ中に分割比率を更新します。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">マウスイベントデータ。</param>
    private void OnSplitterMouseMove(object? sender, MouseEventArgs e)
    {
        if (!_isDraggingSplitter) return;

        var heights = _layout.GetRowHeights();
        if (heights.Length < 3) return;

        var trackHeight = heights[0] + heights[2];
        var delta = _layout.PointToClient(Cursor.Position).Y - _splitterDragStartY;
        ApplyRatio(SplitLayout.RatioFromDrag(_splitterDragStartRatio, delta, trackHeight));
    }

    /// <summary>分割バーのドラッグを終了します。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">マウスイベントデータ。</param>
    private void OnSplitterMouseUp(object? sender, MouseEventArgs e)
    {
        if (!_isDraggingSplitter) return;

        _isDraggingSplitter = false;
        _splitter.Capture = false;
    }

    /// <summary>見出しとテキストボックスを縦に並べたペインを作成します。</summary>
    /// <param name="caption">入力欄の見出し。</param>
    /// <param name="textBox">配置するテキストボックス。</param>
    /// <returns>作成したペイン。</returns>
    private Panel CreatePane(string caption, TextBox textBox)
    {
        var label = new Label
        {
            Text = caption,
            Font = AppTheme.CreateSectionHeaderFont(),
            AutoSize = true,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 0, 0, CaptionSpacing),
        };

        var pane = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
        };
        pane.Controls.Add(textBox);
        pane.Controls.Add(label);
        return pane;
    }

    /// <summary>変換前/変換後のテキストボックスを作成し、ViewModel へバインドします。</summary>
    /// <param name="isEditable">編集を許可する場合は true。</param>
    /// <returns>作成したテキストボックス。</returns>
    private TextBox CreateTextBox(bool isEditable)
    {
        var textBox = new TextBox
        {
            Multiline = true,
            WordWrap = true,
            AcceptsReturn = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            ReadOnly = !isEditable,
            Margin = new Padding(0),
        };

        textBox.DataBindings.Add(
            nameof(TextBox.Text),
            ViewModel,
            isEditable ? nameof(HomeViewModel.BeforeText) : nameof(HomeViewModel.ConvertedText),
            true,
            isEditable ? DataSourceUpdateMode.OnPropertyChanged : DataSourceUpdateMode.Never);

        return textBox;
    }
}
