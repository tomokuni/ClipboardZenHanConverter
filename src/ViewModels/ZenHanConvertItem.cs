using System;
using System.Collections.Concurrent;
using System.ComponentModel;
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
public partial class ZenHanConvertItem : ObservableObject, IDisposable
{
    private readonly ConvertConfig _config;
    private readonly SegmentDefine _def;
    private readonly PropertyInfo _pi;
    private readonly string _propName;

    /// <summary>PropertyInfo のキャッシュ（リフレクション呼び出しを削減）。</summary>
    private static readonly ConcurrentDictionary<string, PropertyInfo> _propertyCache = new();

    /// <summary>項目のラベルを取得します。</summary>
    public string Label => _def.Label;

    /// <summary>コントロールの有効状態を返します。</summary>
    public bool IsEnabled => ForceEnableState ?? true;

    /// <summary>強制的な有効状態設定を取得します。</summary>
    public bool? ForceEnableState => _def.ForceEnableState;

    /// <summary>選択可能なセグメント定義の配列を取得します。</summary>
    public SegmentItem[] Segments { get; }

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

    public ZenHanConvertItem(ConvertConfig config, SegmentDefine def)
    {
        _config = config;
        _def = def;
        _propName = def.Prop;
        _pi = _propertyCache.GetOrAdd(_propName,
            static name => typeof(ConvertConfig).GetProperty(name)
                ?? throw new ArgumentException($"Property {name} not found on ConvertConfig"));

        Segments = def.Segments ??
        [
            new SegmentItem(SettingsModel.TextNone, ZenHanMode.None),
            new SegmentItem(SettingsModel.TextToHan, ZenHanMode.ToHan),
            new SegmentItem(SettingsModel.TextToZen, ZenHanMode.ToZen),
        ];

        _config.PropertyChanged += OnConfigPropertyChanged;
    }

    private void OnConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // 関心のあるプロパティが変更されたときだけ SelectedLabel を再通知
        if (e.PropertyName == _propName || string.IsNullOrEmpty(e.PropertyName))
        {
            OnPropertyChanged(nameof(SelectedLabel));
        }
    }

    public void Dispose()
    {
        _config.PropertyChanged -= OnConfigPropertyChanged;
    }
}
