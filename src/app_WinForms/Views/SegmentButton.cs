namespace ClipboardZenHanConverter.App.WinForms.Views;

/// <summary>排他選択をインラインのボタン列として表示するセグメント用ラジオボタンです。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - 選択状態を背景色（アクセント色）で示すボタン風の外観<br/>
/// - 選択中は文字色を白へ切り替えるコントラストの確保<br/><br/>
/// 特徴: <br/>
/// - <see cref="RadioButton"/> の <see cref="RadioButton.Appearance"/> を <see cref="Appearance.Button"/> にし、
///   丸いラジオマークを描かないセグメント用途の外観にする<br/>
/// - 排他選択は WinForms の既定動作（同一親要素内で排他）に任せるため、
///   行ごとに独立したコンテナへ配置すれば行間で干渉しない<br/>
/// - 幅は内容に合わせて自動調整し、最小幅を他 UI のセグメント（56）に揃える
/// </remarks>
internal sealed class SegmentButton : RadioButton
{
    /// <summary>ボタンの最小幅（論理ピクセル）。</summary>
    private const int MinButtonWidth = 56;

    /// <summary>ボタンの高さ（論理ピクセル）。</summary>
    private const int ButtonHeight = 24;

    /// <summary>左右の余白（論理ピクセル）。</summary>
    private static readonly Padding ButtonPadding = new(10, 0, 10, 0);

    /// <summary>SegmentButton の新しいインスタンスを初期化します。</summary>
    /// <param name="option">表示する選択肢。</param>
    public SegmentButton(SegmentOption option)
    {
        Text = option.Content;
        Appearance = Appearance.Button;
        FlatStyle = FlatStyle.Flat;
        UseVisualStyleBackColor = false;
        TextAlign = ContentAlignment.MiddleCenter;
        Enabled = option.IsEnabled;
        BackColor = Color.Transparent;
        FlatAppearance.BorderSize = 1;
        FlatAppearance.BorderColor = AppTheme.SegmentBorderColor;
        FlatAppearance.CheckedBackColor = AppTheme.AccentColor;
        FlatAppearance.MouseOverBackColor = AppTheme.HoverBackground;
        AutoSize = true;
        MinimumSize = new Size(MinButtonWidth, ButtonHeight);
        Padding = ButtonPadding;
        Margin = new Padding(0, 0, 4, 0);
        UpdateForeColor();
    }

    /// <summary>選択状態が変化したときに文字色を更新します。</summary>
    /// <param name="e">イベントデータ。</param>
    protected override void OnCheckedChanged(EventArgs e)
    {
        base.OnCheckedChanged(e);
        UpdateForeColor();
    }

    /// <summary>選択状態に応じて文字色を切り替えます。</summary>
    /// <remarks>ボタン風の外観では選択中の背景がアクセント色になるため、文字を白にして可読性を確保します。</remarks>
    private void UpdateForeColor()
        => ForeColor = Checked ? AppTheme.AccentForeColor : SystemColors.ControlText;
}
