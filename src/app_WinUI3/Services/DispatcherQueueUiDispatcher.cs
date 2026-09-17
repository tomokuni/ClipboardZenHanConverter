using Microsoft.UI.Dispatching;
using System;

namespace EsUtil.ClipboardZenHanConverter.App.WinUI.Services;

/// <summary>Windows App SDK の DispatcherQueue を用いて UI スレッドへ処理を委譲します。</summary>
/// <remarks>Decorator パターンではありませんが、Adapter として機能し、<br/>
/// DispatcherQueue への依存を本クラスだけに閉じ込めます（他クラスは <see cref="IUiDispatcher"/> を参照）。</remarks>
public sealed class DispatcherQueueUiDispatcher : IUiDispatcher
{
    /// <summary>委譲先のディスパッチキュー。</summary>
    private readonly DispatcherQueue _dispatcherQueue;

    /// <summary>DispatcherQueueUiDispatcher の新しいインスタンスを初期化します。</summary>
    /// <param name="dispatcherQueue">委譲先のディスパッチキュー（null 不可）。</param>
    public DispatcherQueueUiDispatcher(DispatcherQueue dispatcherQueue)
    {
        _dispatcherQueue = dispatcherQueue;
    }

    /// <inheritdoc/>
    public bool TryEnqueue(Action action)
    {
        // DispatcherQueue は優先度付きのオーバーロードしか公開していないため、通常優先度で委譲する
        return _dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => action());
    }
}
