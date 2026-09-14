using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;

namespace EsUtil.ClipboardZenHanConverter.App.AvaloniaUI.Views;

/// <summary>メッセージを表示するシンプルなダイアログです。</summary>
/// <remarks>提供機能: <br/>
/// - 任意のメッセージの表示と OK によるクローズ<br/><br/>
/// 特徴: <br/>
/// - 静的な <see cref="ShowAsync"/> のみを公開し、呼び出し側は表示先のコントロールを渡すだけで済みます
/// </remarks>
public partial class MessageDialog : Window
{
    /// <summary>MessageDialog の新しいインスタンスを初期化します。</summary>
    public MessageDialog()
    {
        InitializeComponent();
    }

    /// <summary>指定されたコントロールを所有ウィンドウとしてメッセージを表示します。</summary>
    /// <param name="owner">所有ウィンドウを決めるためのコントロール。</param>
    /// <param name="message">表示するメッセージ。</param>
    /// <returns>ダイアログが閉じられるまで待機するタスク。</returns>
    public static async Task ShowAsync(Control owner, string message)
    {
        if (TopLevel.GetTopLevel(owner) is not Window parent) return;

        var dialog = new MessageDialog();
        dialog.MessageText.Text = message;
        await dialog.ShowDialog(parent);
    }

    /// <summary>ダイアログを閉じます。</summary>
    /// <param name="sender">イベントソース。</param>
    /// <param name="e">イベントデータ。</param>
    private void OnOkClick(object? sender, RoutedEventArgs e) => Close();
}
