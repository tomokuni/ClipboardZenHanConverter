using ClipboardZenHanConverter.Core.Enums;
using ClipboardZenHanConverter.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace ClipboardZenHanConverter.App.ViewModels;

/// <summary>設定画面の変換項目を表すViewModelクラスです。</summary>
/// <remarks>
/// モデル定義と設定構成に基づいて、UI表示用のプロパティを提供します。<br/>
/// プロパティアクセスは ConvertConfig の型安全な ModePropDef（Get/Set デリゲート）を介するため、<br/>
/// プロパティ名文字列による実行時名前解決・リフレクションに依存しない実装です。
/// </remarks>
public partial class ZenHanConvertItem : ObservableObject, IDisposable
{
    /// <summary>バインド先の変換設定インスタンス。</summary>
    private readonly ConvertConfig _config;
    /// <summary>この項目のセグメント定義（ラベル・型安全なモード定義・セグメント配列）。</summary>
    private readonly SegmentDefine _def;

    /// <summary>項目のラベルを取得します。</summary>
    public string Label => _def.Label;

    /// <summary>コントロールの有効状態を返します。</summary>
    public bool IsEnabled => _def.ForceEnableState ?? true;

    /// <summary>強制的な有効状態設定を取得します。</summary>
    public bool? ForceEnableState => _def.ForceEnableState;

    /// <summary>選択可能なセグメント定義の配列を取得します。</summary>
    public SegmentItem[] Segments { get; }

    /// <summary>現在の設定値に対応するセグメントの表示ラベルを取得または設定します。</summary>
    public string SelectedLabel
    {
        get
        {
            var value = _def.Mode.Get(_config);
            return Segments.FirstOrDefault(x => Equals(x.Value, value))?.Content ?? "";
        }
        set
        {
            var match = Segments.FirstOrDefault(x => x.Content == value);
            if (match is not null && match.IsEnabled)
            {
                var current = _def.Mode.Get(_config);
                if (!Equals(current, match.Value))
                {
                    _def.Mode.Set(_config, match.Value);
                    OnPropertyChanged(nameof(SelectedLabel));
                }
            }
        }
    }

    /// <summary>ZenHanConvertItem の新しいインスタンスを初期化します。</summary>
    /// <param name="config">変換設定。この設定の変更を監視し、SelectedLabel を自動更新します。</param>
    /// <param name="def">セグメント定義（ラベル、型安全なモード定義、セグメント配列）。</param>
    public ZenHanConvertItem(ConvertConfig config, SegmentDefine def)
    {
        _config = config;
        _def = def;

        Segments = def.Segments ??
        [
            new SegmentItem("なし", ZenHanMode.None),
            new SegmentItem("半角", ZenHanMode.ToHan),
            new SegmentItem("全角", ZenHanMode.ToZen),
        ];

        _config.PropertyChanged += OnConfigPropertyChanged;
    }

    /// <summary>ConvertConfig のプロパティ変更時に SelectedLabel を再通知します。</summary>
    /// <remarks>この項目が対象とするモードの値は不変の場合も再通知します。<br/>
    /// 再通知が軽微なため、プロパティ名の照合は行わず常に通知します。</remarks>
    /// <param name="sender">イベントソース（ConvertConfig インスタンス）。</param>
    /// <param name="e">変更されたプロパティ名を含むイベントデータ。</param>
    private void OnConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(SelectedLabel));
    }

    /// <summary>リソースを解放します。ConvertConfig.PropertyChanged の購読を解除します。</summary>
    /// <remarks>Dispose 後はこのインスタンスを使用しないでください。<br/>
    /// 重複呼び出しは安全ですが、パフォーマンス上の理由から避けてください。</remarks>
    public void Dispose()
    {
        _config.PropertyChanged -= OnConfigPropertyChanged;
        GC.SuppressFinalize(this);
    }
}
