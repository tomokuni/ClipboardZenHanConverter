using ClipboardZenHanConverter.App.WinForms.ViewModels;

namespace ClipboardZenHanConverter.App.WinForms.Views;

/// <summary>文字列の置換ルール（追加・編集・削除）を表示するセクションです。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - 置換ルール一覧の表示と各行の編集（検索文字列・置換文字列・正規表現）<br/>
/// - 行の追加と削除<br/>
/// - バリデーションエラーメッセージの表示<br/><br/>
/// 特徴: <br/>
/// - 編集内容は <see cref="ReplacePairItem"/>（INotifyPropertyChanged）へ双方向バインドし、
///   ViewModel 側の検証結果と設定への反映はそのまま利用する<br/>
/// - 一覧の変更（追加・削除・プリセット読み込み）に追従して行を作り直す<br/>
/// - 配置は親の <see cref="SectionStackPanel"/> が行うため、<see cref="Control.Dock"/> は使用しない
/// </remarks>
internal sealed class ReplaceSection : TableLayoutPanel
{
    /// <summary>正規表現列の幅（論理ピクセル）。</summary>
    private const int RegexColumnWidth = 90;

    /// <summary>削除ボタン列の幅（論理ピクセル）。</summary>
    private const int DeleteColumnWidth = 44;

    /// <summary>行間の余白（論理ピクセル）。</summary>
    private const int RowSpacing = 6;

    /// <summary>セクション間の余白（論理ピクセル）。</summary>
    private const int SectionSpacing = 18;

    /// <summary>削除ボタンの表示文字。</summary>
    private const string DeleteButtonText = "✕";

    /// <summary>設定画面の ViewModel。</summary>
    private readonly SettingsViewModel _viewModel;

    /// <summary>置換ルールの行を保持する表。</summary>
    private readonly TableLayoutPanel _rows;

    /// <summary>ReplaceSection の新しいインスタンスを初期化します。</summary>
    /// <param name="viewModel">設定画面の ViewModel。</param>
    public ReplaceSection(SettingsViewModel viewModel)
    {
        _viewModel = viewModel;

        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        ColumnCount = 1;
        ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        Margin = new Padding(0, 0, 0, SectionSpacing);

        AddFullWidthRow(CreateHeaderRow());
        AddFullWidthRow(CreateColumnHeaderRow());

        _rows = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
        };
        _rows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        AddFullWidthRow(_rows);

        _viewModel.ReplaceItems.CollectionChanged += OnReplaceItemsChanged;
        RebuildRows();
    }

    /// <summary>一覧の変更に追従して行を作り直します。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">イベントデータ。</param>
    private void OnReplaceItemsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        => RebuildRows();

    /// <summary>見出し（タイトルと追加ボタン）を作成します。</summary>
    /// <returns>作成した見出し行。</returns>
    private Control CreateHeaderRow()
    {
        var header = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0),
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = "文字列の置換",
            Font = AppTheme.CreateSectionHeaderFont(),
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0),
        };

        var addButton = new Button
        {
            Text = "＋新規",
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(0),
        };
        addButton.Click += (_, _) => _viewModel.AddReplaceRowCommand.Execute(null);

        header.Controls.Add(title, 0, 0);
        header.Controls.Add(addButton, 1, 0);
        return header;
    }

    /// <summary>列見出しの行を作成します。</summary>
    /// <returns>作成した列見出し行。</returns>
    private Control CreateColumnHeaderRow()
    {
        var columns = CreateRowLayout();

        AddColumnHeader(columns, 0, "検索文字列");
        AddColumnHeader(columns, 1, "置換文字列");
        AddColumnHeader(columns, 2, "正規表現");
        AddColumnHeader(columns, 3, "操作");
        return columns;
    }

    /// <summary>列見出しのラベルを追加します。</summary>
    /// <param name="layout">追加先の行レイアウト。</param>
    /// <param name="column">列番号。</param>
    /// <param name="text">表示するテキスト。</param>
    private static void AddColumnHeader(TableLayoutPanel layout, int column, string text)
        => layout.Controls.Add(new Label
        {
            Text = text,
            Font = AppTheme.CreateColumnHeaderFont(),
            AutoSize = true,
            Margin = new Padding(4, 0, 0, 0),
        }, column, 0);

    /// <summary>置換ルールの行を作り直します。</summary>
    private void RebuildRows()
    {
        _rows.SuspendLayout();
        try
        {
            foreach (Control control in _rows.Controls)
                control.Dispose();

            _rows.Controls.Clear();
            _rows.RowStyles.Clear();
            _rows.RowCount = 0;

            foreach (var item in _viewModel.ReplaceItems)
            {
                _rows.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                _rows.RowCount = _rows.RowStyles.Count;
                _rows.Controls.Add(CreateItemRow(item), 0, _rows.RowCount - 1);
            }
        }
        finally
        {
            _rows.ResumeLayout(true);
        }
    }

    /// <summary>置換ルール 1 件分の行を作成します。</summary>
    /// <param name="item">置換ルールの編集項目。</param>
    /// <returns>作成した行。</returns>
    private Control CreateItemRow(ReplacePairItem item)
    {
        var row = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2,
            Margin = new Padding(0, 0, 0, RowSpacing),
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, RegexColumnWidth));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DeleteColumnWidth));
        row.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        row.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        row.Controls.Add(CreateTextBox(item, nameof(ReplacePairItem.Search), "検索文字列"), 0, 0);
        row.Controls.Add(CreateTextBox(item, nameof(ReplacePairItem.Replace), "置換文字列"), 1, 0);
        row.Controls.Add(CreateRegexCheckBox(item), 2, 0);
        row.Controls.Add(CreateDeleteButton(item), 3, 0);

        var error = new Label
        {
            AutoSize = true,
            ForeColor = AppTheme.ErrorForeColor,
            Font = AppTheme.CreateDescriptionFont(),
            Visible = false,
            Margin = new Padding(4, 0, 0, 0),
        };
        item.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(ReplacePairItem.ErrorMessage)) return;
            error.Text = item.ErrorMessage ?? string.Empty;
            error.Visible = !string.IsNullOrEmpty(item.ErrorMessage);
        };

        row.Controls.Add(error, 0, 1);
        row.SetColumnSpan(error, 4);
        return row;
    }

    /// <summary>置換ルールのテキストボックスを作成します。</summary>
    /// <param name="item">置換ルールの編集項目。</param>
    /// <param name="propertyName">バインド先のプロパティ名。</param>
    /// <param name="placeholder">未入力時に表示する説明。</param>
    /// <returns>作成したテキストボックス。</returns>
    private static TextBox CreateTextBox(ReplacePairItem item, string propertyName, string placeholder)
    {
        var textBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 4, 0),
            PlaceholderText = placeholder,
        };
        textBox.DataBindings.Add(nameof(TextBox.Text), item, propertyName, true, DataSourceUpdateMode.OnPropertyChanged);
        return textBox;
    }

    /// <summary>正規表現のチェックボックスを作成します。</summary>
    /// <param name="item">置換ルールの編集項目。</param>
    /// <returns>作成したチェックボックス。</returns>
    private static CheckBox CreateRegexCheckBox(ReplacePairItem item)
    {
        var checkBox = new CheckBox
        {
            Text = "正規表現",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(4, 0, 8, 0),
        };
        checkBox.DataBindings.Add(
            nameof(CheckBox.Checked), item, nameof(ReplacePairItem.IsRegex), true, DataSourceUpdateMode.OnPropertyChanged);
        return checkBox;
    }

    /// <summary>行の削除ボタンを作成します。</summary>
    /// <param name="item">削除対象の置換ルール。</param>
    /// <returns>作成したボタン。</returns>
    private Button CreateDeleteButton(ReplacePairItem item)
    {
        var button = new Button
        {
            Text = DeleteButtonText,
            AutoSize = false,
            Width = 28,
            Height = 24,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0),
        };
        button.Click += (_, _) => _viewModel.DeleteReplaceRowCommand.Execute(item);
        return button;
    }

    /// <summary>置換ルールの行と同じ列構成を持つ表を作成します。</summary>
    /// <returns>作成した表。</returns>
    private static TableLayoutPanel CreateRowLayout()
    {
        var layout = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = new Padding(0, 4, 0, 0),
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, RegexColumnWidth));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DeleteColumnWidth));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        return layout;
    }

    /// <summary>行を全幅で追加します。</summary>
    /// <param name="control">追加するコントロール。</param>
    private void AddFullWidthRow(Control control)
    {
        RowStyles.Add(new RowStyle(SizeType.AutoSize));
        RowCount = RowStyles.Count;
        Controls.Add(control, 0, RowCount - 1);
    }
}
