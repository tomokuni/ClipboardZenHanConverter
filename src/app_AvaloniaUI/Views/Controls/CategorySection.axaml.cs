using Avalonia;
using Avalonia.Controls;
using System.Collections;

namespace ClipboardZenHanConverter.App.AvaloniaUI.Views.Controls;

/// <summary>変換カテゴリ 1 セクション（見出し・説明・変換項目一覧）を表示します。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - カテゴリ見出しと説明の表示<br/>
/// - 変換項目（ラベル + セグメント選択）の一覧表示<br/>
/// - 補足説明（remark）の表示<br/><br/>
/// 特徴: <br/>
/// - 設定画面の 8 カテゴリで同一の表示構造を共有するための再利用コントロール<br/>
/// - 項目の選択は ZenHanConvertItem.Options（SegmentOption）を双方向バインドするため、
///   プリセット読み込みやインポートで設定が外部から変わっても表示が追従する<br/>
/// - 項目一覧は WrapPanel で折り返し、項目数・幅に応じて自然に複数列となる<br/><br/>
/// 注意点: <br/>
/// - 自身のプロパティのみを表示するため、コンストラクタで DataContext を自身に設定します
/// </remarks>
public partial class CategorySection : UserControl
{
    /// <summary>見出しテキストを取得または設定します。</summary>
    /// <value>例: "[ ０ ] 数字の変換"。空の場合は見出しを表示しません。</value>
    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>見出しを取得または設定します（StyledProperty）。</summary>
    public static readonly StyledProperty<string> HeaderProperty =
        AvaloniaProperty.Register<CategorySection, string>(nameof(Header), string.Empty);

    /// <summary>説明テキストを取得または設定します。</summary>
    /// <value>例: "[ 0123456789 ] の数字の変換を指定します。"。空の場合は表示しません。</value>
    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>説明を取得または設定します（StyledProperty）。</summary>
    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<CategorySection, string>(nameof(Description), string.Empty);

    /// <summary>補足説明テキストを取得または設定します。</summary>
    /// <value>例: "※ 半角バックスラッシュは..."。空の場合は表示しません。</value>
    public string Remark
    {
        get => GetValue(RemarkProperty);
        set => SetValue(RemarkProperty, value);
    }

    /// <summary>補足説明を取得または設定します（StyledProperty）。</summary>
    public static readonly StyledProperty<string> RemarkProperty =
        AvaloniaProperty.Register<CategorySection, string>(nameof(Remark), string.Empty);

    /// <summary>表示する変換項目の一覧を取得または設定します。</summary>
    /// <value>ZenHanConvertItem の列挙。null の場合は項目を表示しません。</value>
    public IEnumerable? Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    /// <summary>変換項目一覧を取得または設定します（StyledProperty）。</summary>
    public static readonly StyledProperty<IEnumerable?> ItemsProperty =
        AvaloniaProperty.Register<CategorySection, IEnumerable?>(nameof(Items));

    /// <summary>CategorySection の新しいインスタンスを初期化します。</summary>
    /// <remarks>表示は自身のプロパティを要素名（#Root）で参照するため、DataContext は継承したままにします。</remarks>
    public CategorySection()
    {
        InitializeComponent();
    }
}
