using System;

namespace EsUtil.ZenHanConverter.App.WinUI3.Views.Controls;

/// <summary>分割バーの移動量が変わったことを通知するイベント データです。</summary>
/// <param name="verticalChange">前回の通知からの垂直方向の移動量（下方が正・論理ピクセル）。</param>
public sealed class SplitterDeltaChangedEventArgs(double verticalChange) : EventArgs
{
    /// <summary>前回の通知からの垂直方向の移動量を取得します。</summary>
    /// <value>下方が正の論理ピクセル。</value>
    public double VerticalChange { get; } = verticalChange;
}
