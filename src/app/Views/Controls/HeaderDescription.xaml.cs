using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ClipboardZenHanConverter.App.Views.Controls;

/// <summary>ヘッダーと説明文を表示するユーザーコントロール。</summary>
/// <remarks>ヘッダーテキストと説明文をまとめて表示し、各々にスタイルを適用できます。<br/>
/// 依存関係プロパティ（DependencyProperty）として実装され、バインディングに対応します。<br/>
/// Header と Description の2つのテキスト、および HeaderStyle と DescriptionStyle の2つのスタイルを持ちます。</remarks>
public sealed partial class HeaderDescription : UserControl
{
    /// <summary>HeaderDescription の新しいインスタンスを初期化します。</summary>
    public HeaderDescription()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    /// <summary>コントロール読み込み時にスタイルを適用します。明示的にスタイルが設定されていない場合はデフォルトスタイルを使用します。</summary>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        HeaderText.Style = HeaderStyle ?? (Style)Resources["DefaultHeaderStyle"];
        DescriptionText.Style = DescriptionStyle ?? (Style)Resources["DefaultDescriptionStyle"];
    }

    /// <summary>ヘッダーテキストを取得または設定します。</summary>
    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    /// <summary>Header 依存関係プロパティの識別子。</summary>
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(HeaderDescription), new PropertyMetadata(""));

    /// <summary>説明文を取得または設定します。</summary>
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    /// <summary>Description 依存関係プロパティの識別子。</summary>
    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(HeaderDescription), new PropertyMetadata(""));

    /// <summary>ヘッダーに適用するスタイルを取得または設定します。</summary>
    public Style HeaderStyle
    {
        get => (Style)GetValue(HeaderStyleProperty);
        set => SetValue(HeaderStyleProperty, value);
    }
    /// <summary>HeaderStyle 依存関係プロパティの識別子。</summary>
    public static readonly DependencyProperty HeaderStyleProperty =
        DependencyProperty.Register(nameof(HeaderStyle), typeof(Style), typeof(HeaderDescription), new PropertyMetadata(null));

    /// <summary>説明文に適用するスタイルを取得または設定します。</summary>
    public Style DescriptionStyle
    {
        get => (Style)GetValue(DescriptionStyleProperty);
        set => SetValue(DescriptionStyleProperty, value);
    }
    /// <summary>DescriptionStyle 依存関係プロパティの識別子。</summary>
    public static readonly DependencyProperty DescriptionStyleProperty =
        DependencyProperty.Register(nameof(DescriptionStyle), typeof(Style), typeof(HeaderDescription), new PropertyMetadata(null));
}
