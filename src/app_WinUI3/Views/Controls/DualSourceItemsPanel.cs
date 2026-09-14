// このソースコードは、UTF-8、LF で作成します。

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace EsUtil.ClipboardZenHanConverter.App.WinUI.Views.Controls;

/// <summary>ItemsSource と XAML で直接追加された子要素の両方を扱えるパネルの基底クラス。</summary>
/// <remarks>提供機能: <br/>
/// - ItemsSource（ItemTemplate 併用）と XAML 子要素の同時管理<br/>
/// - ItemsSourceFirst による配置順序の指定<br/>
/// - 子要素の再構築のトリガー（プロパティ変更時と初回 Loaded 時）<br/>
/// - 派生クラス向けのプロパティ変更通知（OnDependencyPropertyChangedInternal 等）<br/><br/>
/// 特徴: <br/>
/// - 子要素の状態は DualSourceItemsManager が単一所有し、本クラスは DependencyProperty のプロキシに徹する<br/>
/// - フォーカス管理は WinUI 3 の標準動作へ委譲し、派生クラスは MeasureOverride / ArrangeOverride に専念できる<br/><br/>
/// 用途: <br/>
/// ItemsControl の ItemsPanel を置き換える派生パネル（MultiColumnPanel 等）の共通基盤として使用する。<br/>
/// </remarks>
public abstract class DualSourceItemsPanel : Panel
{
    /// <summary>ItemsSource 由来の要素と直接子要素を管理するマネージャー。</summary>
    protected readonly DualSourceItemsManager ItemsManager = new();

    /// <summary>パネルの初期化と Loaded イベントの購読を行う。</summary>
    public DualSourceItemsPanel()
    {
        // ItemsSource が後から設定される場合に備え、UI ツリーへ追加された時点で子要素を再構築する
        Loaded += (_, _) => OnPanelLoaded();
    }

    /// <summary>マネージャーが初期化済みかどうかを取得する。</summary>
    /// <value>ItemsSource が設定され、直接子要素が退避済みの場合は true。</value>
    public bool IsManagerInitialized => ItemsManager.HasDirectChildItemsStored;

    /// <summary>ItemsSource と ItemTemplate が両方設定済みかどうかを取得する。</summary>
    /// <value>ItemsPanel として要素を生成できる状態の場合は true。</value>
    public bool IsItemsSourceConnected => ItemsSource is not null && ItemTemplate is not null;

    /// <summary>XAML で直接定義された子要素の数を取得する。</summary>
    /// <value>ItemsSource 設定時に退避した子要素の数（未設定時は 0）。</value>
    public int DirectChildItemsCount => ItemsManager.DirectChildItems.Count;

    /// <summary>アイテムの生成元を指定する DependencyProperty。</summary>
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(object), typeof(DualSourceItemsPanel),
        new PropertyMetadata(null, OnItemsSourceChanged));

    /// <summary>アイテムの表示テンプレートを指定する DependencyProperty。</summary>
    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(DualSourceItemsPanel),
        new PropertyMetadata(null, OnItemTemplateChanged));

    /// <summary>ItemsSource を先に配置するかどうかを指定する DependencyProperty。</summary>
    public static readonly DependencyProperty ItemsSourceFirstProperty = DependencyProperty.Register(
        nameof(ItemsSourceFirst), typeof(bool), typeof(DualSourceItemsPanel),
        new PropertyMetadata(true, OnItemsSourceFirstChanged));

    /// <summary>アイテムの生成元を取得または設定する。</summary>
    /// <value>ItemTemplate で要素化するコレクション（null 可）。</value>
    public object? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>アイテムの表示テンプレートを取得または設定する。</summary>
    /// <value>ItemsSource の各項目を要素化するテンプレート（null 可）。</value>
    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>ItemsSource を直接子要素より先に配置するかどうかを取得または設定する。</summary>
    /// <value>true: ItemsSource → 直接子要素、false: 直接子要素 → ItemsSource（既定値は true）。</value>
    public bool ItemsSourceFirst
    {
        get => ItemsManager.ItemsSourceFirst;
        set => ItemsManager.ItemsSourceFirst = value;
    }

    /// <summary>ItemsSource が変更されたときの処理を行う。</summary>
    /// <param name="d">変更されたパネル（null 不可）。</param>
    /// <param name="e">変更されたプロパティと新旧の値（null 不可）。</param>
    /// <remarks>処理フロー: <br/>
    /// 1. 初回設定の場合、その時点の子要素を直接子要素として退避する<br/>
    /// 2. 派生クラスの拡張ポイントを呼び出す<br/>
    /// 3. 子要素を再構築し、レイアウトを無効化する<br/>
    /// </remarks>
    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var panel = (DualSourceItemsPanel)d;

        // ItemsSource と XAML 子要素の両方を扱うため、初回設定時の子要素を退避しておく
        if (e.OldValue is null && e.NewValue is not null)
        {
            panel.ItemsManager.SaveDirectChildItems(panel.Children);
        }

        panel.OnDependencyPropertyChangedInternal(e);
        panel.OnItemsSourceChangedInternal();
        panel.InvalidateMeasure();
    }

    /// <summary>ItemTemplate が変更されたときの処理を行う。</summary>
    /// <param name="d">変更されたパネル（null 不可）。</param>
    /// <param name="e">変更されたプロパティと新旧の値（null 不可）。</param>
    /// <remarks>処理フロー: <br/>
    /// 1. 派生クラスの拡張ポイントを呼び出す<br/>
    /// 2. 子要素を再構築し、レイアウトを無効化する<br/>
    /// </remarks>
    private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var panel = (DualSourceItemsPanel)d;

        panel.OnDependencyPropertyChangedInternal(e);
        panel.OnItemTemplateChangedInternal();
        panel.InvalidateMeasure();
    }

    /// <summary>ItemsSourceFirst が変更されたときの処理を行う。</summary>
    /// <param name="d">変更されたパネル（null 不可）。</param>
    /// <param name="e">変更されたプロパティと新旧の値（null 不可）。</param>
    /// <remarks>処理フロー: <br/>
    /// 1. マネージャーの配置順序を更新する<br/>
    /// 2. 派生クラスの拡張ポイントを呼び出す<br/>
    /// 3. 子要素を再構築し、レイアウトを無効化する<br/>
    /// </remarks>
    private static void OnItemsSourceFirstChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var panel = (DualSourceItemsPanel)d;

        panel.ItemsManager.ItemsSourceFirst = (bool)e.NewValue;

        panel.OnDependencyPropertyChangedInternal(e);
        panel.OnItemsSourceFirstChangedInternal();
        panel.InvalidateMeasure();
    }

    /// <summary>DependencyProperty が変更されたときに派生クラスへ通知する。</summary>
    /// <param name="e">変更されたプロパティと新旧の値（null 不可）。</param>
    /// <remarks>派生クラスでオーバーライドして監視対象を絞り込む。</remarks>
    protected virtual void OnDependencyPropertyChangedInternal(DependencyPropertyChangedEventArgs e)
    {
    }

    /// <summary>ItemsSource の変更後に派生クラスへ通知する。</summary>
    /// <remarks>既定では子要素を再構築する。派生クラスでオーバーライドして追加処理を行う。</remarks>
    protected virtual void OnItemsSourceChangedInternal()
    {
        RebuildItems();
    }

    /// <summary>ItemTemplate の変更後に派生クラスへ通知する。</summary>
    /// <remarks>既定では子要素を再構築する。派生クラスでオーバーライドして追加処理を行う。</remarks>
    protected virtual void OnItemTemplateChangedInternal()
    {
        RebuildItems();
    }

    /// <summary>ItemsSourceFirst の変更後に派生クラスへ通知する。</summary>
    /// <remarks>既定では子要素を再構築する。派生クラスでオーバーライドして追加処理を行う。</remarks>
    protected virtual void OnItemsSourceFirstChangedInternal()
    {
        RebuildItems();
    }

    /// <summary>ItemsSource と直接子要素から子要素を再構築する。</summary>
    protected void RebuildItems()
    {
        ItemsManager.RebuildChildren(Children, ItemsSource, ItemTemplate);
    }

    /// <summary>UI ツリーへ追加されたときの処理を行う。</summary>
    /// <remarks>ItemsSource が設定される前に XAML 子要素が退避されていた場合に、初回の再構築を行う。</remarks>
    private void OnPanelLoaded()
    {
        if (ItemsManager.HasDirectChildItemsStored)
        {
            RebuildItems();
        }
    }
}
