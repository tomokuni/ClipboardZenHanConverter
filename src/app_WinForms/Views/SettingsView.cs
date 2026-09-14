using EsUtil.ClipboardZenHanConverter.Presentation.ViewModels;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace EsUtil.ClipboardZenHanConverter.App.WinForms.Views;

/// <summary>設定画面を表示するビューです。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - 8 カテゴリの変換設定（セグメント選択）<br/>
/// - 文字列の置換（追加・編集・削除）<br/>
/// - プリセットの選択・編集、設定の JSON エクスポート/インポート<br/><br/>
/// 特徴: <br/>
/// - ファイルの選択は WinForms のファイルダイアログを使い、パスの解決のみを View が担う<br/>
/// - 設定の読み書き自体は ViewModel へ委譲（UI 非依存のロジックとしてテスト可能）<br/>
/// - プリセット選択は <see cref="PresetComboBox"/> をタイトルバーと共有する
/// </remarks>
public sealed class SettingsView : UserControl
{
    /// <summary>見出しの文言。</summary>
    private const string HeaderText = "全角/半角の変換設定";

    /// <summary>見出しの説明文。</summary>
    private const string HeaderDescription = "各文字ごとに全角⇔半角の変換 や 正規化 を行います。";

    /// <summary>JSON ファイルのフィルタ。</summary>
    private const string JsonFilter = "JSON ファイル (*.json)|*.json|すべてのファイル (*.*)|*.*";

    /// <summary>エクスポート時の既定ファイル名。</summary>
    private const string DefaultExportFileName = "ClipboardZenHanConverter_Settings.json";

    /// <summary>設定画面の ViewModel を取得します。</summary>
    public SettingsViewModel ViewModel { get; }

    /// <summary>SettingsView の新しいインスタンスを初期化します。</summary>
    /// <param name="viewModel">設定画面の ViewModel。</param>
    public SettingsView(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        Dock = DockStyle.Fill;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        layout.Controls.Add(CreateHeader(), 0, 0);
        layout.Controls.Add(CreateToolbar(), 0, 1);
        layout.Controls.Add(CreateSections(), 0, 2);

        Controls.Add(layout);
    }

    /// <summary>見出し（タイトルと説明）を作成します。</summary>
    /// <returns>作成した見出し。</returns>
    private static Control CreateHeader()
    {
        var header = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(30, 20, 30, 8),
            Margin = new Padding(0),
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        header.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        header.Controls.Add(new Label
        {
            Text = HeaderText,
            Font = AppTheme.CreateTitleFont(),
            AutoSize = true,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            Margin = new Padding(0, 0, 16, 0),
        }, 0, 0);

        header.Controls.Add(new Label
        {
            Text = HeaderDescription,
            Font = AppTheme.CreateDescriptionFont(),
            ForeColor = AppTheme.SecondaryForeColor,
            AutoSize = true,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            Margin = new Padding(0, 0, 0, 3),
        }, 1, 0);

        return header;
    }

    /// <summary>設定の管理（プリセット・インポート/エクスポート）を作成します。</summary>
    /// <returns>作成したツールバー。</returns>
    private Control CreateToolbar()
    {
        var toolbar = new Panel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
            Padding = new Padding(30, 8, 30, 8),
            Margin = new Padding(0),
        };
        toolbar.Paint += (_, e) => DrawToolbarBorder(toolbar, e);

        var layout = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0),
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(CreatePresetGroup(), 0, 0);
        layout.Controls.Add(CreateExportImportGroup(), 2, 0);

        toolbar.Controls.Add(layout);
        return toolbar;
    }

    /// <summary>ツールバーの上下に区切り線を描画します。</summary>
    /// <param name="toolbar">描画対象のツールバー。</param>
    /// <param name="e">描画イベントデータ。</param>
    private static void DrawToolbarBorder(Control toolbar, PaintEventArgs e)
    {
        using var pen = new Pen(AppTheme.ToolbarBorderColor);
        e.Graphics.DrawLine(pen, 0, 0, toolbar.Width, 0);
        e.Graphics.DrawLine(pen, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
    }

    /// <summary>プリセットの選択と編集のグループを作成します。</summary>
    /// <returns>作成したグループ。</returns>
    private Control CreatePresetGroup()
    {
        var group = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0),
        };

        var combo = new PresetComboBox(ViewModel)
        {
            Width = 220,
            Margin = new Padding(0, 0, 8, 0),
        };

        var editButton = new Button
        {
            Text = "プリセット編集",
            AutoSize = true,
            Margin = new Padding(0),
        };
        editButton.Click += OnEditPresetClick;

        group.Controls.Add(combo);
        group.Controls.Add(editButton);
        return group;
    }

    /// <summary>エクスポート/インポートのグループを作成します。</summary>
    /// <returns>作成したグループ。</returns>
    private Control CreateExportImportGroup()
    {
        var group = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0),
        };

        var exportButton = new Button
        {
            Text = "エクスポート (JSON)",
            AutoSize = true,
            Margin = new Padding(0, 0, 8, 0),
        };
        exportButton.Click += OnExportSettingsClick;

        var importButton = new Button
        {
            Text = "インポート (JSON)",
            AutoSize = true,
            Margin = new Padding(0),
        };
        importButton.Click += OnImportSettingsClick;

        group.Controls.Add(exportButton);
        group.Controls.Add(importButton);
        return group;
    }

    /// <summary>変換設定のセクション一式を作成します。</summary>
    /// <returns>作成したセクション一覧。</returns>
    private Control CreateSections()
    {
        var stack = new SectionStackPanel();

        stack.AddSection(new CategorySection(
            "[ ０ ] 数字の変換",
            "[ 0123456789 ] の数字の変換を指定します。",
            ViewModel.NumberItems));

        stack.AddSection(new CategorySection(
            "[ Ａ ] 英字の変換",
            "[ A-Z a-z ] の英字の変換を指定します。",
            ViewModel.AlphabetItems));

        stack.AddSection(new CategorySection(
            "[ ア ] カナの変換",
            "[ ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾉﾏﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾋﾛﾜｦﾝｧｨｩｪｫｬｭｮｯｰﾞﾟ ] のカナ文字の変換を指定します。",
            ViewModel.KanaItems));

        stack.AddSection(new CategorySection(
            "[ ＠ ] 記号の変換",
            "[ ( ) [ ] { } \" ' , . : ; < = > + - ! # $ % & * / ? @ ^ _ ` | ~ ー 。 、 ] の記号の変換を指定します。",
            ViewModel.SymbolItems));

        stack.AddSection(new CategorySection(
            "[ その他 ] 特殊な文字の変換",
            "[ 長音記号 読点 句点 円記号 スペース 改行 ] などの変換を指定します。",
            ViewModel.EtcZenHanAsciiItems));

        stack.AddSection(new CategorySection(
            null,
            null,
            ViewModel.EtcBslashYenItems,
            "※ 半角バックスラッシュは、フォントによっては 円記号￥の表現になります。"));

        stack.AddSection(new CategorySection(null, null, ViewModel.EtcSpecialItems));
        stack.AddSection(new CategorySection(null, null, ViewModel.EtcMultiSpaceItems));
        stack.AddSection(new ReplaceSection(ViewModel));

        return stack;
    }

    /// <summary>プリセット編集ダイアログを表示します。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">イベントデータ。</param>
    private void OnEditPresetClick(object? sender, EventArgs e)
    {
        using var dialog = new PresetEditDialog(ViewModel);
        dialog.ShowDialog(this);
    }

    /// <summary>現在の設定を JSON ファイルへエクスポートします。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">イベントデータ。</param>
    private void OnExportSettingsClick(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Title = "設定のエクスポート",
            FileName = DefaultExportFileName,
            Filter = JsonFilter,
            DefaultExt = "json",
            AddExtension = true,
        };

        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        ViewModel.ExportSettings(dialog.FileName);
    }

    /// <summary>JSON ファイルから設定をインポートします。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">イベントデータ。</param>
    private void OnImportSettingsClick(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "設定のインポート",
            Filter = JsonFilter,
            CheckFileExists = true,
        };

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        var error = ViewModel.ImportSettings(dialog.FileName);
        if (error is not null)
            MessageDialog.Show(this, error);
    }
}
