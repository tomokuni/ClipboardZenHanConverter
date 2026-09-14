using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using ClipboardZenHanConverter.App.MewUI.ViewModels;
using ClipboardZenHanConverter.Core.Models;
using System.Collections.Specialized;

namespace ClipboardZenHanConverter.App.MewUI.Views;

/// <summary>設定画面を表示するビューです。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - 数字/英字/かな/記号/その他の変換設定（セグメント選択）<br/>
/// - 文字列の置換（ユーザー定義ルール）<br/>
/// - プリセットの保存/読み込み/削除<br/>
/// - 設定のJSONファイルへのエクスポート/インポート<br/>
/// 特徴: <br/>
/// - コードファースト（C# Markup）で構築<br/>
/// - 設定の管理バーはヘッダー直下に固定、カテゴリ部分はスクロール
/// </remarks>
public sealed class SettingsView : UserControl
{
    /// <summary>セグメント1個あたりの左右パディング合計値。</summary>
    private const double PaddingForSegment = 32;

    /// <summary>セグメント間の余白。</summary>
    private const double SegmentsGap = 2;

    /// <summary>変換項目行の行間（DIP）。</summary>
    private const double SegmentRowSpace = 4;

    /// <summary>ラベルの名称部と記号部の間隔（DIP）。</summary>
    private const double LabelSymbolGap = 8;

    /// <summary>ラベル列とセグメントコントロールの間隔（DIP）。</summary>
    private const double LabelSegmentGap = 12;

    /// <summary>マルチカラム表示時の列間（DIP）。移植元の MultiColumnPanel と同一値。</summary>
    private const double SymbolColumnSpace = 48;

    /// <summary>記号の変換で使用する列数上限。移植元の MultiColumnPanel ColumnLimit と同一値。</summary>
    private const int SymbolColumnLimit = 10;

    /// <summary>設定画面の ViewModel。</summary>
    private readonly SettingsViewModel _viewModel;

    /// <summary>SettingsView の新しいインスタンスを初期化します。</summary>
    /// <param name="viewModel">設定画面の ViewModel。</param>
    public SettingsView(SettingsViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    /// <summary>ビューの内容を構築します。</summary>
    /// <returns>設定画面のルート要素。</returns>
    protected override Element? OnBuild() =>
        new Grid()
            .Rows("Auto,Auto,*")
            .Children(
                BuildHeader().Row(0),
                BuildManagementBar().Row(1),
                BuildScrollContent().Row(2)
            );

    /// <summary>ヘッダー（画面見出し）を構築します。</summary>
    /// <returns>ヘッダーの要素。</returns>
    /// <remarks>全角/半角変換の有効/無効スイッチはタイトルバー（MainWindow）が所有するため、ここには配置しません。</remarks>
    private Element BuildHeader() =>
        new StackPanel()
            .Horizontal()
            .Spacing(16)
            .Padding(30, 20, 30, 12)
            .Children(
                new Label()
                    .Text("全角/半角の変換設定")
                    .FontSize(20)
                    .Bold()
                    .CenterVertical(),
                new Label()
                    .Text("各文字ごとに全角⇔半角の変換 や 正規化 を行います。")
                    .CenterVertical()
            );

    /// <summary>設定の管理バー（固定）を構築します。</summary>
    private Element BuildManagementBar() =>
        new Border()
            .Padding(40, 8, 40, 8)
            .Child(
                new Grid()
                    .Columns("*,Auto")
                    .Children(
                        new StackPanel()
                            .Horizontal()
                            .Spacing(8)
                            .Children(
                                new ComboBox()
                                    .Placeholder("プリセットを選択")
                                    .MinWidth(220)
                                    .BindSelectedPreset(_viewModel),
                                new Button()
                                    .Content("プリセット編集")
                                    .OnClick(BuildPresetEditDialog)
                            ),
                        new StackPanel()
                            .Column(1)
                            .Horizontal()
                            .Spacing(8)
                            .Children(
                                new Button()
                                    .Content("エクスポート (JSON)")
                                    .OnClick(OnExportSettings),
                                new Button()
                                    .Content("インポート (JSON)")
                                    .OnClick(OnImportSettings)
                            )
                    )
            );

    /// <summary>スクロール領域（設定カテゴリ + 文字列置換）を構築します。</summary>
    private Element BuildScrollContent() =>
        new ScrollViewer()
            .VerticalScroll(ScrollMode.Auto)
            .Content(
                new StackPanel()
                    .Vertical()
                    .Padding(40, 0, 40, 24)
                    .Spacing(12)
                    .Children(
                        BuildCategorySection("[ ０ ] 数字の変換", "[ 0123456789 ] の数字の変換を指定します。", _viewModel.NumberItems),
                        BuildCategorySection("[ Ａ ] 英字の変換", "[ A-Z a-z ] の英字の変換を指定します。", _viewModel.AlphabetItems),
                        BuildCategorySection("[ ア ] カナの変換", "[ ｱｲｳｴｵ ] のカナ文字の変換を指定します。", _viewModel.KanaItems),
                        BuildMultiColumnCategorySection("[ ＠ ] 記号の変換", "[ ( ) [ ] { } など ] の記号の変換を指定します。", _viewModel.SymbolItems),
                        BuildSpecialCategorySection(),
                        BuildReplaceSection()
                    )
            );

    /// <summary>カテゴリセクションを構築します。</summary>
    /// <param name="header">カテゴリ見出し。</param>
    /// <param name="description">カテゴリの説明文。</param>
    /// <param name="items">カテゴリ内の変換項目。</param>
    /// <returns>カテゴリセクションの要素。</returns>
    /// <remarks>変換項目は単一列（縦並び）で表示します。</remarks>
    private Element BuildCategorySection(string header, string description, IList<ZenHanConvertItem> items)
    {
        var panel = new StackPanel().Vertical().Spacing(SegmentRowSpace);
        AddSegmentRows(panel, items);

        return new GroupBox()
            .Header(header)
            .Content(
                new StackPanel()
                    .Vertical()
                    .Spacing(8)
                    .Children(
                        new Label().Text(description).FontSize(12),
                        panel
                    )
            );
    }

    /// <summary>変換項目が多く縦に長くなるカテゴリのセクションを、マルチカラムで構築します。</summary>
    /// <param name="header">カテゴリ見出し。</param>
    /// <param name="description">カテゴリの説明文。</param>
    /// <param name="items">カテゴリ内の変換項目。</param>
    /// <returns>カテゴリセクションの要素。</returns>
    /// <remarks>処理フロー: <br/>
    /// 1. 変換項目行を生成（名称部は自然幅、記号部とセグメントは最大幅に固定）<br/>
    /// 2. 行を MultiColumnPanel へ列数上限付きで配置し、最大列高さを最小化<br/><br/>
    /// 注意点: <br/>
    /// - 行は列幅へ広がるため、記号部とセグメントコントロールは各列の右端で縦に揃います</remarks>
    private Element BuildMultiColumnCategorySection(string header, string description, IList<ZenHanConvertItem> items)
    {
        var rows = BuildSegmentRows(items, SegmentRowAlignment.ColumnEdge);

        var panel = new MultiColumnPanel
        {
            ColumnLimit = SymbolColumnLimit,
            RowSpace = SegmentRowSpace,
            ColumnSpace = SymbolColumnSpace,
        }.Children([.. rows]);

        return new GroupBox()
            .Header(header)
            .Content(
                new StackPanel()
                    .Vertical()
                    .Spacing(8)
                    .Children(
                        new Label().Text(description).FontSize(12),
                        panel
                    )
            );
    }

    /// <summary>その他の特殊文字セクションを構築します。</summary>
    private Element BuildSpecialCategorySection() =>
        new GroupBox()
            .Header("[ その他 ] 特殊な文字の変換")
            .Content(
                new StackPanel()
                    .Vertical()
                    .Spacing(8)
                    .Children(
                        new Label()
                            .Text("[ 長音記号 読点 句点 円記号 スペース 改行 ] などの変換を指定します。")
                            .FontSize(12),
                        BuildRows(_viewModel.EtcZenHanAsciiItems),
                        BuildRows(_viewModel.EtcBslashYenItems),
                        new Label()
                            .Text("※ 半角バックスラッシュは、フォントによっては 円記号￥の表現になります。")
                            .FontSize(12),
                        BuildRows(_viewModel.EtcSpecialItems),
                        BuildRows(_viewModel.EtcMultiSpaceItems)
                    )
            );

    /// <summary>指定された変換項目群をグループとして縦に並べた要素を構築します。</summary>
    private Element BuildRows(IList<ZenHanConvertItem> items)
    {
        var panel = new StackPanel().Vertical().Spacing(SegmentRowSpace);
        AddSegmentRows(panel, items);
        return panel;
    }

    /// <summary>グループ内の全変換項目行を生成し、パネルへ追加します。</summary>
    /// <param name="panel">行を追加するパネル。</param>
    /// <param name="items">グループ内の変換項目。</param>
    private void AddSegmentRows(Panel panel, IList<ZenHanConvertItem> items)
    {
        foreach (var row in BuildSegmentRows(items, SegmentRowAlignment.FixedLabelWidth))
            panel.Add(row);
    }

    /// <summary>変換項目行の整列方法。</summary>
    private enum SegmentRowAlignment
    {
        /// <summary>名称部の幅をグループ内の最大値に固定する。行は内容幅のまま左寄せとなり、記号部とセグメントコントロールの横位置が揃う（単一列セクション用）。</summary>
        FixedLabelWidth,

        /// <summary>名称部は自然幅のまま、記号部を可変幅の列に置いて行の右端へ寄せる。行が列幅へ広がるマルチカラムでは、記号部とセグメントコントロールが列の右端で揃う。</summary>
        ColumnEdge,
    }

    /// <summary>グループ内の全変換項目行を生成します。記号部とセグメントの幅をグループ内の最大値に揃えます。</summary>
    /// <param name="items">グループ内の変換項目。</param>
    /// <param name="alignment">行の整列方法。</param>
    /// <returns>変換項目行の一覧。</returns>
    /// <remarks>処理フロー: <br/>
    /// 1. 整列方法に応じて名称部の幅とセグメントテキストの最大幅を計測<br/>
    /// 2. 各項目行を生成（記号部・セグメントの幅を最大値に固定）<br/><br/>
    /// 注意点: <br/>
    /// - ColumnEdge では名称部が自然幅のため行幅が名称の長さに応じて変わり、MultiColumnPanel が
    ///   列ごとの最大行幅として列幅を算出します。</remarks>
    private List<Element> BuildSegmentRows(IList<ZenHanConvertItem> items, SegmentRowAlignment alignment)
    {
        var nameWidth = alignment == SegmentRowAlignment.FixedLabelWidth
            ? SegmentItems.GetMaxNameWidth(items)
            : 0;
        var symbolWidth = SegmentItems.GetMaxSymbolWidth(items);
        var segmentTextWidth = SegmentItems.GetMaxSegmentTextWidth(items);

        var rows = new List<Element>(items.Count);

        foreach (var item in items)
        {
            var segmentWidth = CalcSegmentWidth(item, segmentTextWidth);
            rows.Add(BuildSegmentRow(item, nameWidth, symbolWidth, segmentWidth, alignment));
        }

        return rows;
    }

    /// <summary>セグメントコントロール全体の幅を、グループ内最大セグメントテキスト幅に基づいて算出します。</summary>
    /// <param name="item">変換項目。</param>
    /// <param name="segmentTextWidth">グループ内のセグメントテキスト最大幅。</param>
    /// <returns>セグメントコントロールの全体幅。</returns>
    /// <remarks>全てのセグメントを同一幅（最大テキスト幅）に揃えるため、SegmentSizing.Uniform を前提に算出します。<br/>
    /// セグメント個数とテキスト幅の他に、内部パディング・区切りの余白を加算します。</remarks>
    private static double CalcSegmentWidth(ZenHanConvertItem item, double segmentTextWidth)
    {
        // セグメント1個あたりの幅 = 最大テキスト幅 + 左右パディング
        var perSegmentWidth = segmentTextWidth + PaddingForSegment;
        return perSegmentWidth * item.Options.Count + SegmentsGap * (item.Options.Count - 1);
    }

    /// <summary>文字列置換セクションを構築します。</summary>
    private Element BuildReplaceSection()
    {
        var rowsPanel = new StackPanel().Vertical().Spacing(4);

        void RebuildRows()
        {
            ViewExtensions.RebuildReplaceRows(rowsPanel, _viewModel);
        }
        RebuildRows();
        _viewModel.ReplaceItems.CollectionChanged += (_, _) => RebuildRows();

        return new GroupBox()
            .Header("文字列の置換")
            .Content(
                new StackPanel()
                    .Vertical()
                    .Spacing(8)
                    .Children(
                        new Grid()
                            .Columns("*,*,Auto")
                            .Children(
                                new Label().Text("検索").Bold(),
                                new Label().Column(1).Text("置換").Bold(),
                                new Label().Column(2).Text("正規表現").Bold()
                            ),
                        rowsPanel,
                        new Button()
                            .Content("行を追加")
                            .OnClick(() => _viewModel.AddReplaceRowCommand.Execute(null))
                    )
            );
    }

    /// <summary>単一のセグメント行（ラベル + セグメントコントロール）を構築します。</summary>
    /// <param name="item">変換項目。</param>
    /// <param name="nameWidth">グループ内で揃える名称部の幅（0 の場合は名称部を自然幅とする）。</param>
    /// <param name="symbolWidth">グループ内で揃える記号部の幅。</param>
    /// <param name="segmentWidth">グループ内で揃えるセグメントコントロールの幅。</param>
    /// <param name="alignment">行の整列方法。</param>
    /// <returns>セグメント行の要素。</returns>
    /// <remarks>処理フロー: <br/>
    /// 1. ラベルを名称部と記号部に分割し、記号部を最大記号幅の枠へ中央寄せにする<br/>
    /// 2. FixedLabelWidth では名称部の幅を固定し、行を内容幅のまま左寄せに整列<br/>
    /// 3. ColumnEdge では名称部を自然幅のまま、記号部の枠を行の右端へ寄せる<br/><br/>
    /// 注意点: <br/>
    /// - ColumnEdge の行は、MultiColumnPanel が列幅（列ごとの最大行幅）で配置することを前提とします</remarks>
    private Element BuildSegmentRow(ZenHanConvertItem item, double nameWidth, double symbolWidth, double segmentWidth, SegmentRowAlignment alignment)
    {
        var segmented = new SegmentedControl { ItemsSource = SegmentItems.Create(item.Options) };

        // 表示中の選択位置。プリセット読み込み・インポートなど外部から設定が変わった場合も
        // この値を更新してセグメントの表示を追従させる。
        var selectedIndex = new ObservableValue<int>(SegmentItems.GetSelectedIndex(item), v => v);

        segmented
            .Sizing(SegmentSizing.Uniform)
            .BindSelectedIndex(selectedIndex)
            .OnSelectionChanged(_ =>
            {
                var selected = segmented.SelectedText;
                if (selected is not null)
                    item.SelectedLabel = selected;
            });

        // 項目とビューは共にアプリケーションと同じ寿命のため、購読の解除は不要
        item.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(ZenHanConvertItem.SelectedLabel) or null or "")
                selectedIndex.Value = SegmentItems.GetSelectedIndex(item);
        };

        // グループ内の最大幅に固定し、画面幅いっぱいには広げない。
        var segmentedHost = new Grid()
            .Children(segmented)
            .Width(segmentWidth)
            .HorizontalAlignment(HorizontalAlignment.Left)
            .IsEnabled(item.IsEnabled);

        var (name, symbol) = SegmentItems.SplitLabel(item.Label);

        var nameLabel = new Label().Text(name).CenterVertical();
        var symbolLabel = new Label()
            .Text(symbol)
            .MinWidth(symbolWidth)
            .TextAlignment(TextAlignment.Center)
            .CenterVertical();

        if (alignment == SegmentRowAlignment.ColumnEdge)
        {
            // 記号部は最大記号幅の固定枠へ中央寄せで置き、枠ごと行の右端へ寄せる。
            // 行が列幅へ広がるため、枠とセグメントコントロールは各列の右端で縦に揃う。
            var symbolSlot = new Grid()
                .Children(symbolLabel)
                .Width(symbolWidth)
                .HorizontalAlignment(HorizontalAlignment.Right);

            return new Grid()
                .Columns("Auto,*,Auto")
                .Spacing(LabelSegmentGap)
                .Children(
                    nameLabel,
                    symbolSlot.Column(1),
                    segmentedHost.Column(2)
                );
        }

        // 名称部の幅を固定し、行は内容幅のまま左寄せに整列する。
        var labelArea = new Grid()
            .Columns("Auto,Auto")
            .Spacing(LabelSymbolGap)
            .Children(
                nameLabel
                    .MinWidth(nameWidth)
                    .HorizontalAlignment(HorizontalAlignment.Left),
                symbolLabel.Column(1)
            );

        return new Grid()
            .Columns("Auto,Auto")
            .Spacing(LabelSegmentGap)
            .Children(
                labelArea,
                segmentedHost.Column(1)
            );
    }

    /// <summary>プリセット編集ダイアログを構築・表示します。</summary>
    private void BuildPresetEditDialog()
    {
        var name = new ObservableValue<string>(_viewModel.SelectedPresetName ?? string.Empty);
        var dialog = new Window()
            .Title("プリセット編集")
            .Padding(16);
        dialog.Content(
            new StackPanel()
                .Vertical()
                .Spacing(12)
                .Children(
                    new Label().Text("プリセット名").Bold(),
                    new TextBox()
                        .BindText(name)
                        .Placeholder("プリセット名を入力"),
                    new StackPanel()
                        .Horizontal()
                        .Spacing(8)
                        .Children(
                            new Button()
                                .Content("保存")
                                .OnClick(() =>
                                {
                                    _viewModel.SavePreset(name.Value);
                                    dialog.Close();
                                }),
                            new Button()
                                .Content("削除")
                                .OnClick(() =>
                                {
                                    _viewModel.DeletePreset(name.Value);
                                    dialog.Close();
                                }),
                            new Button()
                                .Content("閉じる")
                                .OnClick(() => dialog.Close())
                        )
                )
        );

        var mainWindow = Global.MainWindow;
        if (mainWindow is null) dialog.Show();
        else dialog.ShowDialogAsync(mainWindow);
    }

    /// <summary>設定をJSONファイルとしてエクスポートします。</summary>
    private void OnExportSettings()
    {
        var mainWindow = Global.MainWindow;
        var file = FileDialog.SaveFile(new SaveFileDialogOptions
        {
            Owner = mainWindow,
            Title = "設定のエクスポート",
            Filters = FileFilter.Parse("JSON ファイル (*.json)|*.json"),
            FileName = "settings.json",
            DefaultExtension = ".json",
        });
        if (file is null) return;
        _viewModel.ExportSettings(file);
    }

    /// <summary>JSONファイルから設定をインポートします。</summary>
    private void OnImportSettings()
    {
        var mainWindow = Global.MainWindow;
        var file = FileDialog.OpenFile(new OpenFileDialogOptions
        {
            Owner = mainWindow,
            Title = "設定のインポート",
            Filters = FileFilter.Parse("JSON ファイル (*.json)|*.json"),
        });
        if (file is null) return;
        var error = _viewModel.ImportSettings(file);
        if (error is not null)
            ShowError(error);
    }

    /// <summary>エラーメッセージを表示します。オーナーウィンドウが利用可能な場合はメッセージボックスを使用します。</summary>
    /// <param name="message">表示するメッセージ。</param>
    private void ShowError(string message)
    {
        var mainWindow = Global.MainWindow;
        if (mainWindow is not null)
            NativeMessageBox.Show(mainWindow.Handle, message, "エラー", NativeMessageBoxButtons.Ok, NativeMessageBoxIcon.Error);
    }
}
