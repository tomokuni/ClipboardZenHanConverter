using System.ComponentModel;
using System.Linq;
using ClipboardZenHanConverter.App.Models;
using ClipboardZenHanConverter.Core.Enums;
using ClipboardZenHanConverter.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardZenHanConverter.App.ViewModels;

/// <summary>設定画面の変換項目を表すViewModelクラスです。</summary>
/// <remarks>
/// モデル定義と設定構成に基づいて、UI表示用のプロパティを提供します。<br/>
/// </remarks>
public partial class ZenHanConvertItem : ObservableObject, IDisposable
{
    /// <summary>バインド先の変換設定インスタンス。</summary>
    private readonly ConvertConfig _config;
    /// <summary>この項目のセグメント定義（ラベル・プロパティ名・セグメント配列）。</summary>
    private readonly SegmentDefine _def;
    /// <summary>ConvertConfig から現在値を取得するデリゲート（リフレクション生成）。</summary>
    private readonly Func<ConvertConfig, object?> _getValue;
    /// <summary>ConvertConfig に選択値を設定するデリゲート（リフレクション生成）。</summary>
    private readonly Action<ConvertConfig, object?> _setValue;

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
            var value = _getValue(_config);
            return Segments.FirstOrDefault(x => Equals(x.Value, value))?.Content ?? "";
        }
        set
        {
            var match = Segments.FirstOrDefault(x => x.Content == value);
            if (match is not null && match.IsEnabled)
            {
                var current = _getValue(_config);
                if (!Equals(current, match.Value))
                {
                    _setValue(_config, match.Value);
                    OnPropertyChanged(nameof(SelectedLabel));
                }
            }
        }
    }

    /// <summary>ZenHanConvertItem の新しいインスタンスを初期化します。</summary>
    /// <param name="config">変換設定。この設定の変更を監視し、SelectedLabel を自動更新します。</param>
    /// <param name="def">セグメント定義（ラベル、バインド先プロパティ、セグメント配列）。</param>
    /// <exception cref="ArgumentException">def.Prop が ConvertConfig に存在しない場合。</exception>
    public ZenHanConvertItem(ConvertConfig config, SegmentDefine def)
    {
        _config = config;
        _def = def;
        var pi = typeof(ConvertConfig).GetProperty(def.Prop)
            ?? throw new ArgumentException($"Property {def.Prop} not found on ConvertConfig");
        _getValue = c => pi.GetValue(c);
        _setValue = (c, v) => pi.SetValue(c, v);

        Segments = def.Segments ??
        [
            new SegmentItem("なし", ZenHanMode.None),
            new SegmentItem("半角", ZenHanMode.ToHan),
            new SegmentItem("全角", ZenHanMode.ToZen),
        ];

        _config.PropertyChanged += OnConfigPropertyChanged;
    }

    /// <summary>ConvertConfig のプロパティ変更時に SelectedLabel を再通知します。</summary>
    /// <remarks>関心のあるプロパティ（_def.Prop）が変更された場合のみ通知することで無駄な再描画を防止します。</remarks>
    private void OnConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == _def.Prop || string.IsNullOrEmpty(e.PropertyName))
        {
            OnPropertyChanged(nameof(SelectedLabel));
        }
    }

    /// <summary>リソースを解放します。ConvertConfig.PropertyChanged の購読を解除します。</summary>
    public void Dispose()
    {
        _config.PropertyChanged -= OnConfigPropertyChanged;
        GC.SuppressFinalize(this);
    }
}
