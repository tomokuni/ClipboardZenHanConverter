using ClipboardZenHanConverter.App.WinForms.ViewModels;

namespace ClipboardZenHanConverter.App.WinForms.Views;

/// <summary>変換カテゴリ 1 セクション（見出し・説明・変換項目一覧）を表示します。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - カテゴリ見出しと説明の表示<br/>
/// - 変換項目（ラベル + セグメント選択）の一覧表示<br/>
/// - 補足説明（remark）の表示<br/><br/>
/// 特徴: <br/>
/// - 設定画面の 8 カテゴリで同一の表示構造を共有するための再利用コントロール<br/>
/// - 項目の選択は <see cref="ZenHanConvertItem.Options"/>（<see cref="SegmentOption"/>）を購読し、
///   プリセット読み込みやインポートで設定が外部から変わっても表示が追従する<br/>
/// - 名称列は AutoSize とし、グループ内で最も長い名称に合わせて幅が決まる（見切れず、選択ボタンの横位置も揃う）<br/>
/// - 見出し・説明・補足は表の全列を結合して配置し、折り返し幅をセクション幅に追従させる<br/><br/>
/// 注意点: <br/>
/// - 変換項目は 1 行に 1 つ配置します（Avalonia UI 版の折り返し配置とは異なるが、項目の並び順は同一）<br/>
/// - 配置は親の <see cref="SectionStackPanel"/> が行うため、<see cref="Control.Dock"/> は使用しません
/// </remarks>
internal sealed class CategorySection : TableLayoutPanel
{
    /// <summary>名称列の左右の余白（論理ピクセル）。</summary>
    private const int LabelColumnPadding = 16;

    /// <summary>説明の折り返し幅を決める左右の余白（論理ピクセル）。</summary>
    private const int HorizontalPadding = 4;

    /// <summary>項目行の上下の余白（論理ピクセル）。</summary>
    private const int ItemRowPadding = 2;

    /// <summary>セクション間の余白（論理ピクセル）。</summary>
    private const int SectionSpacing = 18;

    /// <summary>折り返し幅を追従させる説明・補足ラベル。</summary>
    private readonly List<Label> _wrappingLabels = [];

    /// <summary>CategorySection の新しいインスタンスを初期化します。</summary>
    /// <param name="header">見出しテキスト。例: "[ ０ ] 数字の変換"。空の場合は表示しません。</param>
    /// <param name="description">説明テキスト。空の場合は表示しません。</param>
    /// <param name="items">表示する変換項目の一覧。</param>
    /// <param name="remark">補足説明テキスト。null または空の場合は表示しません。</param>
    public CategorySection(string? header, string? description, IEnumerable<ZenHanConvertItem> items, string? remark = null)
    {
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        ColumnCount = 2;
        ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        Margin = new Padding(0, 0, 0, SectionSpacing);

        if (!string.IsNullOrEmpty(header))
            AddSpanning(header, AppTheme.CreateSectionHeaderFont(), SystemColors.ControlText);

        if (!string.IsNullOrEmpty(description))
            AddSpanning(description, AppTheme.CreateDescriptionFont(), AppTheme.SecondaryForeColor);

        foreach (var item in items)
            AddItemRow(item);

        if (!string.IsNullOrEmpty(remark))
            AddSpanning(remark, AppTheme.CreateDescriptionFont(), AppTheme.SecondaryForeColor);
    }

    /// <summary>セクションの幅が変化したときに、説明・補足の折り返し幅を更新します。</summary>
    /// <param name="e">イベントデータ。</param>
    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);

        var width = Math.Max(0, ClientSize.Width - (HorizontalPadding * 2));
        foreach (var label in _wrappingLabels)
            label.MaximumSize = new Size(width, 0);
    }

    /// <summary>行を追加して、その行番号を返します。</summary>
    /// <returns>追加した行の番号。</returns>
    /// <remarks><see cref="RowStyles"/> と <see cref="TableLayoutPanel.RowCount"/> を常に一致させます。</remarks>
    private int AddRow()
    {
        RowStyles.Add(new RowStyle(SizeType.AutoSize));
        RowCount = RowStyles.Count;
        return RowCount - 1;
    }

    /// <summary>表の全列を結合した行を追加します。</summary>
    /// <param name="text">表示するテキスト。</param>
    /// <param name="font">使用するフォント。</param>
    /// <param name="foreColor">文字色。</param>
    private void AddSpanning(string text, Font font, Color foreColor)
    {
        var label = new Label
        {
            Text = text,
            Font = font,
            ForeColor = foreColor,
            AutoSize = true,
            Margin = new Padding(HorizontalPadding, ItemRowPadding, HorizontalPadding, ItemRowPadding),
        };

        _wrappingLabels.Add(label);

        var row = AddRow();
        Controls.Add(label, 0, row);
        SetColumnSpan(label, 2);
    }

    /// <summary>変換項目 1 行（ラベル + セグメント選択）を追加します。</summary>
    /// <param name="item">変換項目。</param>
    private void AddItemRow(ZenHanConvertItem item)
    {
        var label = new Label
        {
            Text = item.Label,
            Font = AppTheme.CreateLabelFont(),
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 0, LabelColumnPadding, 0),
        };

        var options = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0),
        };

        foreach (var option in item.Options)
        {
            var button = new SegmentButton(option)
            {
                Checked = option.IsSelected,
            };
            button.DataBindings.Add(
                nameof(RadioButton.Checked), option, nameof(SegmentOption.IsSelected), true, DataSourceUpdateMode.OnPropertyChanged);
            options.Controls.Add(button);
        }

        var row = AddRow();
        Controls.Add(label, 0, row);
        Controls.Add(options, 1, row);
    }
}
