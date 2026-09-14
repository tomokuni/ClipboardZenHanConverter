using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace EsUtil.ClipboardZenHanConverter.App.WinForms.Views;

/// <summary>セクションを縦に並べ、内容に応じて縦スクロールを提供するパネルです。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - 追加されたセクションを上から順に縦一列で配置<br/>
/// - 幅に合わせた折り返しを反映するため、セクションの高さを再計算して配置<br/>
/// - 内容が収まらない場合の縦スクロール<br/><br/>
/// 特徴: <br/>
/// - <see cref="Control.Dock"/> は追加順と z 順の関係が分かりにくいため使わず、
///   セクションの位置とサイズを明示的に計算して配置する<br/>
/// - セクションの幅は常に表示領域の幅に合わせるため、説明文の折り返し幅が追従する
/// </remarks>
internal sealed class SectionStackPanel : Panel
{
    /// <summary>左右の余白（論理ピクセル）。</summary>
    private const int HorizontalPadding = 30;

    /// <summary>上下の余白（論理ピクセル）。</summary>
    private const int VerticalPadding = 14;

    /// <summary>配置対象のセクション（追加順）。</summary>
    private readonly List<Control> _sections = [];

    /// <summary>SectionStackPanel の新しいインスタンスを初期化します。</summary>
    public SectionStackPanel()
    {
        Dock = DockStyle.Fill;
        AutoScroll = true;
    }

    /// <summary>セクションを末尾へ追加し、配置を更新します。</summary>
    /// <param name="section">追加するセクション。</param>
    public void AddSection(Control section)
    {
        _sections.Add(section);
        Controls.Add(section);
        LayoutSections();
    }

    /// <summary>表示領域のサイズ変化時にセクションを再配置します。</summary>
    /// <param name="e">イベントデータ。</param>
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        LayoutSections();
    }

    /// <summary>セクションを上から順に縦一列で配置します。</summary>
    private void LayoutSections()
    {
        var width = ClientSize.Width - (HorizontalPadding * 2);
        if (width <= 0)
            return;

        var y = VerticalPadding;
        foreach (var section in _sections)
        {
            var height = section.GetPreferredSize(new Size(width, 0)).Height;
            section.Bounds = new Rectangle(HorizontalPadding, y, width, height);
            y += height + section.Margin.Bottom;
        }
    }
}
