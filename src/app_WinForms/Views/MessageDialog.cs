using System.Drawing;
using System.Windows.Forms;

namespace EsUtil.ClipboardZenHanConverter.App.WinForms.Views;

/// <summary>メッセージを表示するシンプルなダイアログです。</summary>
/// <remarks>提供機能: <br/>
/// - 任意のメッセージの表示と OK によるクローズ<br/><br/>
/// 特徴: <br/>
/// - 静的な <see cref="Show(IWin32Window, string)"/> のみを公開し、呼び出し側は表示先を渡すだけで済みます
/// </remarks>
internal sealed class MessageDialog : Form
{
    /// <summary>ダイアログの余白（論理ピクセル）。</summary>
    private static readonly Padding DialogPadding = new(20);

    /// <summary>ダイアログの幅（論理ピクセル）。</summary>
    private const int DialogWidth = 420;

    /// <summary>メッセージを表示するラベルの最大幅（論理ピクセル）。</summary>
    private const int MessageMaxWidth = 360;

    /// <summary>MessageDialog の新しいインスタンスを初期化します。</summary>
    /// <param name="message">表示するメッセージ。</param>
    private MessageDialog(string message)
    {
        Text = "ClipboardZenHanConverter";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        MinimizeBox = false;
        MaximizeBox = false;
        AutoScaleMode = AutoScaleMode.Font;
        AutoScaleDimensions = new SizeF(7F, 15F);

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

        layout.Controls.Add(new Label
        {
            Text = message,
            AutoSize = true,
            MaximumSize = new Size(MessageMaxWidth, 0),
            Margin = new Padding(0, 0, 0, 16),
        });

        var okButton = new Button
        {
            Text = "OK",
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(0),
        };
        okButton.Click += (_, _) => Close();
        layout.Controls.Add(okButton);

        Controls.Add(layout);
        layout.PerformLayout();
        ClientSize = new Size(DialogWidth, layout.PreferredSize.Height);
    }

    /// <summary>指定された所有者を親としてメッセージを表示します。</summary>
    /// <param name="owner">所有ウィンドウを決めるためのコントロール。</param>
    /// <param name="message">表示するメッセージ。</param>
    /// <remarks>モーダル表示のため、呼び出し側は閉じられるまで待機します。</remarks>
    public static void Show(IWin32Window owner, string message)
    {
        using var dialog = new MessageDialog(message);
        dialog.ShowDialog(owner);
    }
}
