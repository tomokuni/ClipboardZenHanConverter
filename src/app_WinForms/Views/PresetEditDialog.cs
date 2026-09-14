using ClipboardZenHanConverter.App.WinForms.ViewModels;

namespace ClipboardZenHanConverter.App.WinForms.Views;

/// <summary>プリセットの保存・削除を行うダイアログです。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - プリセット名の入力とリアルタイム検証<br/>
/// - 保存 / 削除 / 閉じる<br/><br/>
/// 特徴: <br/>
/// - 検証と操作は <see cref="PresetEditDialogViewModel"/> が保持し、View は表示のみを担う<br/>
/// - 保存・削除の可否は ViewModel の CanSave / CanDelete を各ボタンの有効/無効へバインドする
/// </remarks>
public sealed class PresetEditDialog : Form
{
    /// <summary>ダイアログの余白（論理ピクセル）。</summary>
    private static readonly Padding DialogPadding = new(20);

    /// <summary>要素間の余白（論理ピクセル）。</summary>
    private const int ElementSpacing = 12;

    /// <summary>ダイアログの幅（論理ピクセル）。</summary>
    private const int DialogWidth = 420;

    /// <summary>入力欄の下に表示する補足説明。</summary>
    private const string RemarkText = "既存のユーザープリセット名を入力すると上書き保存できます。";

    /// <summary>プリセット名の入力欄に表示する説明。</summary>
    private const string PresetNamePlaceholder = "保存するプリセット名を入力します";

    /// <summary>プリセット編集の ViewModel を取得します。</summary>
    public PresetEditDialogViewModel ViewModel { get; }

    /// <summary>PresetEditDialog の新しいインスタンスを初期化します。</summary>
    /// <param name="settings">設定画面の ViewModel。</param>
    public PresetEditDialog(SettingsViewModel settings)
    {
        ViewModel = new PresetEditDialogViewModel(settings);
        ViewModel.SetCloseAction(Close);

        Text = "プリセット編集";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        MinimizeBox = false;
        MaximizeBox = false;
        AutoScaleMode = AutoScaleMode.Font;
        AutoScaleDimensions = new SizeF(7F, 15F);
        ClientSize = new Size(DialogWidth, 0);

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = DialogPadding,
            Margin = new Padding(0),
        };

        layout.Controls.Add(CreateTitleLabel());
        layout.Controls.Add(CreateNameTextBox());
        layout.Controls.Add(CreateErrorLabel());
        layout.Controls.Add(CreateRemarkLabel());
        layout.Controls.Add(CreateButtonRow());

        Controls.Add(layout);
        layout.PerformLayout();
        ClientSize = new Size(DialogWidth, layout.PreferredSize.Height);
    }

    /// <summary>プリセット名の見出しを作成します。</summary>
    /// <returns>作成したラベル。</returns>
    private static Label CreateTitleLabel() => new()
    {
        Text = "プリセット名",
        Font = AppTheme.CreateSectionHeaderFont(),
        AutoSize = true,
        Margin = new Padding(0, 0, 0, ElementSpacing),
    };

    /// <summary>プリセット名の入力欄を作成します。</summary>
    /// <returns>作成したテキストボックス。</returns>
    private TextBox CreateNameTextBox()
    {
        var textBox = new TextBox
        {
            Width = DialogWidth - DialogPadding.Horizontal,
            PlaceholderText = PresetNamePlaceholder,
            Margin = new Padding(0, 0, 0, ElementSpacing),
        };
        textBox.DataBindings.Add(
            nameof(TextBox.Text), ViewModel, nameof(PresetEditDialogViewModel.PresetName), true, DataSourceUpdateMode.OnPropertyChanged);
        return textBox;
    }

    /// <summary>検証エラーメッセージのラベルを作成します。</summary>
    /// <returns>作成したラベル。</returns>
    private Label CreateErrorLabel()
    {
        var label = new Label
        {
            Font = AppTheme.CreateDescriptionFont(),
            ForeColor = AppTheme.ErrorForeColor,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, ElementSpacing),
        };
        label.DataBindings.Add(
            nameof(Label.Text), ViewModel, nameof(PresetEditDialogViewModel.ErrorMessage), true, DataSourceUpdateMode.Never);
        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PresetEditDialogViewModel.ErrorMessage))
                label.Visible = !string.IsNullOrEmpty(ViewModel.ErrorMessage);
        };
        label.Visible = false;
        return label;
    }

    /// <summary>補足説明のラベルを作成します。</summary>
    /// <returns>作成したラベル。</returns>
    private static Label CreateRemarkLabel() => new()
    {
        Text = RemarkText,
        Font = AppTheme.CreateDescriptionFont(),
        ForeColor = AppTheme.SecondaryForeColor,
        AutoSize = true,
        Margin = new Padding(0, 0, 0, ElementSpacing),
    };

    /// <summary>操作ボタンの行を作成します。</summary>
    /// <returns>作成した行。</returns>
    private Control CreateButtonRow()
    {
        var row = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(0),
        };

        row.Controls.Add(CreateButton("保存", nameof(PresetEditDialogViewModel.CanSave), () => ViewModel.SaveCommand.Execute(null)));
        row.Controls.Add(CreateButton("削除", nameof(PresetEditDialogViewModel.CanDelete), () => ViewModel.DeleteCommand.Execute(null)));
        row.Controls.Add(CreateButton("閉じる", null, () => ViewModel.CloseCommand.Execute(null)));
        return row;
    }

    /// <summary>操作ボタンを作成します。</summary>
    /// <param name="text">ボタンの表示文字。</param>
    /// <param name="enabledProperty">有効状態をバインドするプロパティ名。null の場合は常に有効。</param>
    /// <param name="action">クリック時の処理。</param>
    /// <returns>作成したボタン。</returns>
    private Button CreateButton(string text, string? enabledProperty, Action action)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            Margin = new Padding(8, 0, 0, 0),
        };

        if (enabledProperty is not null)
        {
            button.DataBindings.Add(
                nameof(Button.Enabled), ViewModel, enabledProperty, true, DataSourceUpdateMode.Never);
        }

        button.Click += (_, _) => action();
        return button;
    }
}
