// このソースコードは、UTF-8、LF で作成します。

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EsUtil.ClipboardZenHanConverter.App.WinUI.Views.Controls;

/// <summary>ItemsSource 由来の要素と XAML で直接追加された子要素を併せて管理する。</summary>
/// <remarks>提供機能: <br/>
/// - ItemsSource が最初に設定された時点の子要素の退避（直接子要素）<br/>
/// - ItemsSource と ItemTemplate からの要素生成<br/>
/// - ItemsSourceFirst に従った配置順序での子要素の再構築<br/><br/>
/// 特徴: <br/>
/// - 子要素の状態を単一所有し、パネル側は本クラスへ委譲する<br/>
/// - 直接子要素の退避は一度のみ行うため、以降の再構築で XAML 定義の子要素が失われない<br/><br/>
/// 用途: <br/>
/// DualSourceItemsPanel が内包し、派生パネルが ItemsControl 互換の子要素管理を得るために使用する。<br/>
/// </remarks>
public sealed class DualSourceItemsManager
{
    /// <summary>ItemsSource を先に配置するかどうか。</summary>
    private bool _itemsSourceFirst = true;

    /// <summary>直接子要素を退避済みかどうか（空の場合でも退避済みを区別する）。</summary>
    private bool _isDirectChildItemsSaved;

    /// <summary>退避した直接子要素。</summary>
    private List<UIElement> _directChildItems = [];

    /// <summary>直接子要素を退避済みかどうかを取得する。</summary>
    /// <value>ItemsSource が最初に設定されるまでは false。</value>
    public bool HasDirectChildItemsStored => _isDirectChildItemsSaved;

    /// <summary>退避した直接子要素を取得する。</summary>
    /// <value>ItemsSource 設定前の XAML 子要素（未退避の場合は空）。</value>
    public IReadOnlyList<UIElement> DirectChildItems => _directChildItems.AsReadOnly();

    /// <summary>ItemsSource を直接子要素より先に配置するかどうかを取得または設定する。</summary>
    /// <value>true: ItemsSource → 直接子要素、false: 直接子要素 → ItemsSource。</value>
    public bool ItemsSourceFirst
    {
        get => _itemsSourceFirst;
        set => _itemsSourceFirst = value;
    }

    /// <summary>ItemsSource が最初に設定された時点の子要素を直接子要素として退避する。</summary>
    /// <param name="currentChildren">現在の子要素コレクション（null 不可）。</param>
    /// <remarks>処理フロー: <br/>
    /// 1. 退避済みの場合は何もしない（再構築のたびに上書きしないため）<br/>
    /// 2. 現在の子要素を UIElement として複製し、退避済みとする<br/>
    /// </remarks>
    public void SaveDirectChildItems(UIElementCollection currentChildren)
    {
        if (_isDirectChildItemsSaved)
        {
            return;
        }

        _directChildItems = [.. currentChildren.OfType<UIElement>()];
        _isDirectChildItemsSaved = true;
    }

    /// <summary>ItemsSource と ItemTemplate から要素を生成する。</summary>
    /// <param name="itemsSource">要素の生成元（null 可）。</param>
    /// <param name="itemTemplate">要素のテンプレート（null 可）。</param>
    /// <returns>生成した要素のリスト（生成できない場合は空）。</returns>
    /// <remarks>処理フロー: <br/>
    /// 1. ItemsSource または ItemTemplate が未設定の場合は空リストを返す<br/>
    /// 2. ItemsSource の各項目から DataTemplate.LoadContent() で要素を生成する<br/>
    /// 3. 生成した要素へ項目を DataContext として設定する<br/>
    /// </remarks>
    public static List<FrameworkElement> GenerateItemsSourceElements(object? itemsSource, DataTemplate? itemTemplate)
    {
        var itemsSourceElements = new List<FrameworkElement>();

        if (itemTemplate is null || itemsSource is null)
        {
            return itemsSourceElements;
        }

        if (itemsSource is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (itemTemplate.LoadContent() is FrameworkElement el)
                {
                    el.DataContext = item;
                    itemsSourceElements.Add(el);
                }
            }
        }

        return itemsSourceElements;
    }

    /// <summary>ItemsSource 由来の要素と直接子要素を、指定されたコレクションへ配置順に追加する。</summary>
    /// <param name="targetCollection">子要素を再構築する対象コレクション（null 不可）。</param>
    /// <param name="itemsSourceElements">ItemsSource から生成済みの要素（null 不可）。</param>
    /// <remarks>処理フロー: <br/>
    /// 1. 対象コレクションをクリアする<br/>
    /// 2. ItemsSourceFirst に従い、ItemsSource 由来の要素と直接子要素を順に追加する<br/>
    /// </remarks>
    public void RebuildChildren(UIElementCollection targetCollection, List<FrameworkElement> itemsSourceElements)
    {
        targetCollection.Clear();

        IReadOnlyList<UIElement> first = _itemsSourceFirst ? itemsSourceElements : _directChildItems;
        IReadOnlyList<UIElement> second = _itemsSourceFirst ? _directChildItems : itemsSourceElements;

        for (int i = 0; i < first.Count; i++)
        {
            targetCollection.Add(first[i]);
        }

        for (int i = 0; i < second.Count; i++)
        {
            targetCollection.Add(second[i]);
        }
    }

    /// <inheritdoc cref="RebuildChildren(UIElementCollection, List{FrameworkElement})"/>
    /// <param name="targetCollection">子要素を再構築する対象コレクション（null 不可）。</param>
    /// <param name="itemsSource">要素の生成元（null 可）。</param>
    /// <param name="itemTemplate">要素のテンプレート（null 可）。</param>
    public void RebuildChildren(UIElementCollection targetCollection, object? itemsSource, DataTemplate? itemTemplate)
    {
        var itemsSourceElements = GenerateItemsSourceElements(itemsSource, itemTemplate);

        RebuildChildren(targetCollection, itemsSourceElements);
    }

    /// <summary>保持している状態を初期状態へ戻す。</summary>
    /// <remarks>処理フロー: <br/>
    /// 1. 退避した直接子要素を破棄する<br/>
    /// 2. 退避済みフラグと ItemsSourceFirst を既定値へ戻す<br/>
    /// </remarks>
    public void Reset()
    {
        _directChildItems.Clear();
        _isDirectChildItemsSaved = false;
        _itemsSourceFirst = true;
    }
}
