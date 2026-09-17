using System;

namespace EsUtil.ClipboardZenHanConverter.App.WinUI.Services;

/// <summary>UI スレッドへ処理を委譲する抽象化を提供します。</summary>
/// <remarks>DIP に従い、ViewModels はこのインターフェースを通じて UI スレッドへ処理を依頼します。<br/>
/// Windows App SDK の型（DispatcherQueue 等）を ViewModel の公開シグネチャへ出さないための境界であり、<br/>
/// UI を持たないテストが Windows App Runtime を必要としないようにする役割も持ちます。</remarks>
public interface IUiDispatcher
{
    /// <summary>UI スレッドでアクションを実行します。</summary>
    /// <param name="action">UI スレッドで実行するアクション（null 不可）。</param>
    /// <returns>受付けられた場合は true。UI スレッドが終了している等で受付けられない場合は false。</returns>
    /// <remarks>呼び出し元のスレッドが UI スレッドであるかは問いません。</remarks>
    bool TryEnqueue(Action action);
}
