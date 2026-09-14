using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardZenHanConverter.Presentation.ViewModels;

/// <summary>セグメント選択の選択肢 1 件を表します。</summary>
/// <remarks>排他選択の表示状態を 1 つの bool（<see cref="IsSelected"/>）で持ちます。<br/>
/// 選択状態の同期は所有側の <see cref="ZenHanConvertItem"/> が行います。<br/>
/// UI フレームワークによって排他選択の手段が異なるため、両方の使い方に対応できる形にしています。<br/><br/>
/// 1. 選択肢ごとにコントロールを並べる UI（RadioButton 等）は <see cref="IsSelected"/> を双方向で参照します。<br/>
/// 2. 選択肢を 1 つのコントロールで表す UI（セグメントコントロール等）は
///    親の <see cref="ZenHanConvertItem.SelectedLabel"/> を参照します。
/// </remarks>
public sealed partial class SegmentOption : ObservableObject
{
    /// <summary>表示テキストを取得します。</summary>
    /// <value>「なし」「半角」「全角」などの表示ラベル。</value>
    public string Content { get; }

    /// <summary>排他選択のグループ名を取得します。</summary>
    /// <value>同じ行内の選択肢で同一、行間では異なる値。</value>
    /// <remarks>ラジオボタンのグループ名に指定し、行ごとに独立した排他選択にします。</remarks>
    public string GroupName { get; }

    /// <summary>この選択肢が有効かどうかを取得します。</summary>
    /// <value>変換対象外などで選択できない場合は false。</value>
    public bool IsEnabled { get; }

    /// <summary>この選択肢が選択されているかどうかを取得または設定します。</summary>
    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    /// <summary>SegmentOption の新しいインスタンスを初期化します。</summary>
    /// <param name="content">表示テキスト。</param>
    /// <param name="groupName">排他選択のグループ名。</param>
    /// <param name="isEnabled">この選択肢が有効かどうか。</param>
    public SegmentOption(string content, string groupName, bool isEnabled = true)
    {
        Content = content;
        GroupName = groupName;
        IsEnabled = isEnabled;
    }
}
