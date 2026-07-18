using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;

namespace ClipboardZenHanConverter.App.Views.Controls;

/// <summary>行番号を表示するテキストボックスコントロール。</summary>
/// <remarks>改行コード(CRLF / CR / LF)で行数をカウントし、左ガターに行番号を表示します。<br/>
/// TextBox の内部 ScrollViewer とガターのスクロール位置を同期します。<br/>
/// 依存関係プロパティとして Text / IsReadOnly を公開し、バインディングに対応します。</remarks>
public sealed partial class LineNumberTextBox : UserControl
{
    private ScrollViewer? _textBoxScrollViewer;

    /// <summary>LineNumberTextBox の新しいインスタンスを初期化します。</summary>
    public LineNumberTextBox()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    /// <summary>コントロール読み込み時に TextBox の内部 ScrollViewer を取得し、スクロール同期を設定します。</summary>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _textBoxScrollViewer = FindScrollViewer(ContentTextBox);
        if (_textBoxScrollViewer is not null)
        {
            _textBoxScrollViewer.ViewChanged += OnTextBoxScrollViewChanged;
        }

        UpdateLineNumbers();
    }

    /// <summary>テキスト変更時に呼び出され、行番号を再計算します。</summary>
    private void OnContentTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateLineNumbers();
    }

    /// <summary>テキストを改行コード(CRLF / CR / LF)で分割し、行番号テキストを更新します。</summary>
    private void UpdateLineNumbers()
    {
        if (string.IsNullOrEmpty(Text))
        {
            GutterText.Text = "";
            return;
        }

        // 改行コードを正規化: CRLF -> LF, CR -> LF
        var normalized = Text.Replace("\r\n", "\n").Replace('\r', '\n');
        var lineCount = normalized.Split('\n').Length;
        GutterText.Text = string.Join("\n", Enumerable.Range(1, lineCount).Select(i => i.ToString()));
    }

    /// <summary>TextBox のスクロール位置変更時にガターのスクロール位置を同期します。</summary>
    private void OnTextBoxScrollViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
    {
        if (_textBoxScrollViewer is null)
            return;

        // ガターの ScrollViewer を TextBox のスクロール位置に同期
        GutterScrollViewer.ChangeView(null, _textBoxScrollViewer.VerticalOffset, null);
    }

    /// <summary>ビジュアルツリーを走査して指定された DependencyObject 内の ScrollViewer を取得します。</summary>
    private static ScrollViewer? FindScrollViewer(DependencyObject? parent)
    {
        if (parent is null) return null;

        var childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is ScrollViewer sv)
                return sv;

            var result = FindScrollViewer(child);
            if (result is not null)
                return result;
        }
        return null;
    }

    /// <summary>表示テキストを取得または設定します。</summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Text 依存関係プロパティの識別子。</summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(LineNumberTextBox),
            new PropertyMetadata("", OnTextPropertyChanged));

    /// <summary>Text プロパティ変更時にガターの行番号を更新します。</summary>
    private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LineNumberTextBox control && e.NewValue is string newText)
        {
            // XAML の x:Bind で既に ContentTextBox.Text が更新されるが、
            // コードからの Text プロパティ変更時にも行番号を更新する
            control.UpdateLineNumbers();
        }
    }

    /// <summary>読み取り専用かを取得または設定します。</summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>IsReadOnly 依存関係プロパティの識別子。</summary>
    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(LineNumberTextBox),
            new PropertyMetadata(true));
}
