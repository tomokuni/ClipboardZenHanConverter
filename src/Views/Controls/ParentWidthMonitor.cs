using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using ClipboardZenHanConverter.Core.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace ClipboardZenHanConverter.Views.Controls;

/// <summary>
/// 親要素の幅変更を監視するためのインターフェース。
/// テスト容易性を高めるために導入されました。
/// </summary>
public interface IParentWidthMonitor
{
    /// <summary>
    /// 親要素の幅が変更されたときに発生するイベント。
    /// </summary>
    event EventHandler<SizeChangedEventArgs>? OnParentWidthChanged;

    /// <summary>
    /// 現在の幅を取得します。
    /// </summary>
    double CurrentWidth { get; }
}

/// <summary>
/// 親要素の幅変更を監視するクラス。
/// ViewModel を導入してサイズ監視を宣言的に扱います。
/// </summary>
public partial class ParentWidthMonitor : ObservableObject, IParentWidthMonitor
{
    /// <summary>
    /// 監視対象の要素（弱参照）。
    /// </summary>
    protected readonly WeakReference<FrameworkElement> _targetElement;

    /// <summary>
    /// 現在の親要素（弱参照）。
    /// </summary>
    protected WeakReference<FrameworkElement>? _parentElement;

    /// <summary>
    /// サイズ変更の閾値。デフォルトは 0.5。
    /// </summary>
    public double SizeChangeThreshold { get; set; } = 0.5;

    /// <summary>
    /// 親要素の幅が変更されたときに発生するイベント。
    /// </summary>
    public event EventHandler<SizeChangedEventArgs>? OnParentWidthChanged;

    /// <summary>
    /// 現在の幅を取得または設定します。
    /// </summary>
    [ObservableProperty]
    public partial double CurrentWidth { get; set; }

    /// <summary>
    /// 指定された要素を監視対象として初期化します。
    /// </summary>
    /// <param name="target">監視対象の FrameworkElement。</param>
    /// <exception cref="ArgumentNullException">target が null の場合にスローされます。</exception>
    public ParentWidthMonitor(FrameworkElement target)
    {
        _targetElement = new WeakReference<FrameworkElement>(target ?? throw new ArgumentNullException(nameof(target)));

        // 要素が読み込まれたときに親要素の監視を開始
        target.Loaded += OnTargetLoaded;

        // 要素がアンロードされたときに監視を停止
        //target.Unloaded += OnTargetUnloaded;

        // 自身のサイズ変更を監視して親要素の変更を検知
        target.SizeChanged += OnSizeChanged;

        // 初期幅を設定
        CurrentWidth = target.Width;
    }

    /// <summary>
    /// 対象要素が読み込まれたときのイベントハンドラー。
    /// 親要素の監視を開始します。
    /// </summary>
    private void OnTargetLoaded(object? sender, RoutedEventArgs e)
    {
        if (_targetElement.TryGetTarget(out _))
        {
            SubscribeToParent();
        }
    }

    /// <summary>
    /// 対象要素がアンロードされたときのイベントハンドラー。
    /// 監視を停止します。
    /// </summary>
    private void OnTargetUnloaded(object? sender, RoutedEventArgs e)
    {
        if (_targetElement.TryGetTarget(out var target))
        {
            target.SizeChanged -= OnSizeChanged;
        }
        UnsubscribeFromParent();
    }

    /// <summary>
    /// 対象要素のサイズが変更されたときのイベントハンドラー。
    /// 親要素が変更された場合、監視を再設定します。
    /// </summary>
    protected void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (_targetElement.TryGetTarget(out var target))
        {
            if (_parentElement?.TryGetTarget(out var currentParent) != true || currentParent != target.Parent)
            {
                UnsubscribeFromParent();
                SubscribeToParent();
            }
        }
    }

    /// <summary>
    /// 親要素の監視を開始します。
    /// </summary>
    public void SubscribeToParent()
    {
        if (_targetElement.TryGetTarget(out var target) && target.Parent is FrameworkElement parent)
        {
            _parentElement = new WeakReference<FrameworkElement>(parent);
            parent.SizeChanged += OnParentWidthPreChanged;
        }
    }

    /// <summary>
    /// 親要素の監視を停止します。
    /// </summary>
    public void UnsubscribeFromParent()
    {
        if (_parentElement?.TryGetTarget(out var parent) == true)
        {
            parent.SizeChanged -= OnParentWidthPreChanged;
        }
        _parentElement = null;
    }

    /// <summary>
    /// 親要素の幅が変更されたときのイベントハンドラー。
    /// 閾値を超えた場合にのみイベントを発火します。
    /// </summary>
    protected void OnParentWidthPreChanged(object sender, SizeChangedEventArgs e)
    {
        var diffWidth = Math.Abs(e.NewSize.Width - e.PreviousSize.Width);
        if (diffWidth < SizeChangeThreshold) return;

        CurrentWidth = e.NewSize.Width;
        OnParentWidthChanged?.Invoke(sender, e);
    }

    /// <summary>
    /// 指定された要素のサイズ情報をテキスト形式で取得します。
    /// </summary>
    /// <param name="element">サイズ情報を取得する FrameworkElement。</param>
    /// <returns>フォーマットされたサイズ情報文字列。</returns>
    public static string GetSizeInfoText(FrameworkElement element)
    {
        var parentInnerSize = Format(element.GetInnerSize());
        var parentSize = Format(new Size(element.Width, element.Height));
        var parentActualSize = Format(element.ActualSize.ToSize());
        var parentMargin = Format(element.Margin);
        var parentPadding = Format(element.GetPadding());

        return $@"
描画可能な内側サイズ（概算）: {parentInnerSize}
  Size: {parentSize}   |   ActualSize: {parentActualSize}   |   Margin: {parentMargin}   |   Padding: {parentPadding}
".Trim();
    }

    /// <summary>
    /// Thickness をフォーマットされた文字列に変換します。
    /// 値が等しい場合は簡略化して表示します。
    /// </summary>
    /// <param name="thickness">変換する Thickness。</param>
    /// <returns>フォーマットされた文字列。</returns>
    public static string Format(Thickness thickness) =>
        thickness is { Left: var l, Top: var t, Right: var r, Bottom: var b } &&
        l == t && r == b && l == r
            ? $"{l:F1}"
            : l == r && t == b
                ? $"{l:F1}, {t:F1}"
                : $"L:{l:F1}, T:{t:F1}, R:{r:F1}, B:{b:F1}";

    /// <summary>
    /// Size をフォーマットされた文字列に変換します。
    /// NaN の場合は "Auto" と表示します。
    /// </summary>
    /// <param name="size">変換する Size。</param>
    /// <returns>フォーマットされたサイズ文字列。</returns>
    public static string Format(Size size) =>
        $"{(double.IsNaN(size.Width) ? "Auto" : $"{size.Width:F1}")} × {(double.IsNaN(size.Height) ? "Auto" : $"{size.Height:F1}")}";
}


#region ParentWidthMonitorSampleControl

/// <summary>
/// ParentWidthMonitor を使用したサンプルコントロール。
/// 親要素の幅変更をデバッグ情報として表示します。
/// </summary>
public partial class ParentWidthMonitorSampleControl : UserControl, INotifyPropertyChanged
{
    #region PropertyChanged, SetProperty

    /// <summary>
    /// プロパティ変更イベント。
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// プロパティ変更を通知します。
    /// </summary>
    /// <param name="propertyName">変更されたプロパティ名。</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// プロパティの値を設定し、変更を通知します。
    /// </summary>
    /// <typeparam name="T">プロパティの型。</typeparam>
    /// <param name="backingField">バッキングフィールド。</param>
    /// <param name="value">新しい値。</param>
    /// <param name="propertyName">プロパティ名。</param>
    /// <returns>値が変更された場合は true。</returns>
    protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingField, value)) return false;
        backingField = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion

    #region INotifyPropertyChanged Property

    public string DebugInfo
    {
        get => field;
        set => SetProperty(ref field, value);
    } = "待機中...";

    #endregion

    /// <summary>
    /// 親幅監視インスタンス。
    /// </summary>
    protected readonly IParentWidthMonitor _parentWidthMonitor;

    /// <summary>
    /// デフォルトコンストラクタ。依存性注入なし。
    /// </summary>
    public ParentWidthMonitorSampleControl() : this(null)
    { }

    /// <summary>
    /// 依存性注入用のコンストラクタ。
    /// </summary>
    /// <param name="parentWidthMonitor">注入する IParentWidthMonitor インスタンス。</param>
    /// <exception cref="ArgumentNullException">parentWidthMonitor が null の場合にスローされます。</exception>
    public ParentWidthMonitorSampleControl(IParentWidthMonitor? monitor = null)
    {
        _parentWidthMonitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        _parentWidthMonitor.OnParentWidthChanged += OnParentWidthChanged;
        Loaded += (_, _) => UpdateDebugInfo();
    }

    protected void OnParentWidthChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateDebugInfo();
    }

    protected void UpdateDebugInfo()
    {
        var text = $"【このコントロール  {GetType().Name}】  {ParentWidthMonitor.GetSizeInfoText(this)}";
        if (Parent is not FrameworkElement parent)
            text += "\n\n【親コントロール】\n親要素が見つかりません";
        else
            text += $"\n\n【親コントロール  {parent.GetType().Name}  x:Name=\"{parent.Name}\"】  {ParentWidthMonitor.GetSizeInfoText(parent)}";
        DebugInfo = text;
    }
}

#endregion
