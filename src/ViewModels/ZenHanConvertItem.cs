using System;
using System.Linq;
using System.Reflection;
using ClipboardZenHanConverter.Core.Enums;
using ClipboardZenHanConverter.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardZenHanConverter.ViewModels;

/// <summary>設定画面の変換項目を表すViewModelクラスです。</summary>
/// <remarks>
/// モデル定義と設定構成に基づいて、UI表示用のプロパティを提供します。<br/>
/// </remarks>
public partial class ZenHanConvertItem(ConvertConfig config, SegmentDefine def) : ObservableObject
{
    private readonly ConvertConfig _config = config;
    private readonly SegmentDefine _def = def;
    private readonly PropertyInfo _pi = typeof(ConvertConfig).GetProperty(def.Prop) 
        ?? throw new ArgumentException($"Property {def.Prop} not found on ConvertConfig");


    /// <summary>項目のラベルを取得します。</summary>
    public string Label => _def.Label;
    
    /// <summary>コントロールの有効状態を返します。</summary>
    public bool IsEnabled => ForceEnableState ?? true;

    /// <summary>強制的な有効状態設定を取得します。</summary>
    public bool? ForceEnableState => _def.ForceEnableState;

    /// <summary>選択可能なセグメント定義の配列を取得します。</summary>
    public SegmentItem[] Segments { get; } = def.Segments ??
        [
            new SegmentItem(SettingsModel.TextNone, ZenHanMode.None),
            new SegmentItem(SettingsModel.TextToHan, ZenHanMode.ToHan),
            new SegmentItem(SettingsModel.TextToZen, ZenHanMode.ToZen),
        ];

    /// <summary>選択されている項目のラベルを取得または設定します。</summary>
    public string SelectedLabel
    {
        get
        {
            var value = _pi.GetValue(_config);
            return Segments.FirstOrDefault(x => Equals(x.Value, value))?.Content ?? "";
        }
        set
        {
            var match = Segments.FirstOrDefault(x => x.Content == value);
            if (match is not null && match.IsEnabled)
            {
                var current = _pi.GetValue(_config);
                if (!Equals(current, match.Value))
                {
                    _pi.SetValue(_config, match.Value);
                    OnPropertyChanged(nameof(SelectedLabel));
                }
            }
        }
    }
}
