using EsUtil.ClipboardZenHanConverter.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace EsUtil.ClipboardZenHanConverter.Tests.Parity;

/// <summary>テスト用のクリップボードサービス（各アプリが共有するコア インターフェースの実装）。</summary>
/// <remarks>各 UI 実装の ViewModel を同じ入力で比較するため、パリティテスト全体で共有します。</remarks>
internal sealed class FakeClipboardService : IClipboardService
{
    /// <summary>取得対象のテキスト。</summary>
    public string? Text { get; set; }

    /// <summary>最後に設定されたテキスト。</summary>
    public string? LastSetText { get; private set; }

    /// <summary>SetText の呼び出し回数。</summary>
    public int SetTextCount { get; private set; }

    /// <summary>Flush の呼び出し回数。</summary>
    public int FlushCount { get; private set; }

    /// <inheritdoc/>
    public event EventHandler<object>? ContentChanged;

    /// <inheritdoc/>
    public Task<string?> GetTextAsync() => Task.FromResult(Text);

    /// <inheritdoc/>
    public void SetText(string text)
    {
        SetTextCount++;
        LastSetText = text;
    }

    /// <inheritdoc/>
    public void Flush() => FlushCount++;

    /// <inheritdoc/>
    public void Dispose() { }

    /// <summary>クリップボード内容の変更をシミュレートします。</summary>
    public void RaiseContentChanged() => ContentChanged?.Invoke(this, EventArgs.Empty);
}
