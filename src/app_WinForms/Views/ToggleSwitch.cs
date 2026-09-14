using System;
using System.Drawing;
using System.Windows.Forms;

namespace EsUtil.ClipboardZenHanConverter.App.WinForms.Views;

/// <summary>クリップボード変換の有効/無効を切り替えるトグルスイッチです。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - トグルスイッチ風の外観によるオン/オフの表示<br/>
/// - オン時にノブを右端へ移動し、トラックをアクセント色で塗る<br/><br/>
/// 特徴: <br/>
/// - WinForms にはトグルスイッチが無いため、<see cref="CheckBox"/> を自前描画して他の 3 つの UI と外観を揃える<br/>
/// - 操作（クリック・キーボード）は <see cref="CheckBox"/> の既定動作をそのまま利用する
/// </remarks>
internal sealed class ToggleSwitch : CheckBox
{
    /// <summary>ノブの色。</summary>
    private static readonly Color KnobColor = Color.White;

    /// <summary>ToggleSwitch の新しいインスタンスを初期化します。</summary>
    public ToggleSwitch()
    {
        AutoSize = false;
        Text = string.Empty;
        Size = new Size(AppTheme.SwitchTrackWidth, AppTheme.SwitchTrackHeight);
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    /// <summary>表示に必要なサイズを返します。スイッチ本体のサイズは固定です。</summary>
    /// <param name="proposedSize">親から提案されたサイズ。</param>
    /// <returns>トラックのサイズ。</returns>
    public override Size GetPreferredSize(Size proposedSize)
        => new(AppTheme.SwitchTrackWidth, AppTheme.SwitchTrackHeight);

    /// <summary>チェック状態が変化したときに再描画します。</summary>
    /// <param name="e">イベントデータ。</param>
    protected override void OnCheckedChanged(EventArgs e)
    {
        base.OnCheckedChanged(e);
        Invalidate();
    }

    /// <summary>スイッチを描画します。</summary>
    /// <param name="e">描画イベントデータ。</param>
    /// <remarks>寸法はコントロールの実サイズに対する比率で求めるため、DPI スケーリングに自動で追従します。</remarks>
    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // トラックの外側（四隅）は描画対象外のため、残像が残らないよう背景で塗りつぶす
        using (var backgroundBrush = new SolidBrush(BackColor))
            graphics.FillRectangle(backgroundBrush, ClientRectangle);

        // 枠線を内側に収めるため 1px 引いた範囲へ描く
        var track = new Rectangle(0, 0, Width - 1, Height - 1);
        if (track.Width <= 0 || track.Height <= 0)
            return;

        var radius = track.Height / 2;
        var trackColor = Checked ? AppTheme.AccentColor : AppTheme.SwitchOffTrackColor;

        using (var trackPath = CreateRoundedRectangle(track, radius))
        using (var trackBrush = new SolidBrush(trackColor))
        {
            graphics.FillPath(trackBrush, trackPath);
        }

        // ノブの余白は設計値（高さ 20 に対する 3）の比率で求める
        var inset = Math.Max(1, (int)Math.Round(track.Height * AppTheme.SwitchKnobInset / (double)AppTheme.SwitchTrackHeight));
        var knobSize = track.Height - (2 * inset);
        var knobX = Checked ? track.Right - inset - knobSize : track.Left + inset;
        var knob = new Rectangle(knobX, track.Top + inset, knobSize, knobSize);

        using var knobBrush = new SolidBrush(KnobColor);
        graphics.FillEllipse(knobBrush, knob);
    }

    /// <summary>角の丸い矩形のパスを作成します。</summary>
    /// <param name="bounds">矩形の範囲。</param>
    /// <param name="radius">角の半径。</param>
    /// <returns>角を丸めた矩形のパス。</returns>
    private static System.Drawing.Drawing2D.GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        var diameter = radius * 2;

        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 90, 180);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 180);
        path.CloseFigure();
        return path;
    }
}
