using System;
using System.Threading.Tasks;

namespace ClipboardZenHanConverter.Core.Services;

/// <summary>クリップボード操作を抽象化するインターフェース</summary>
public interface IClipboardService
{
    event EventHandler<object> ContentChanged;

    /// <summary>クリップボードのテキストを取得します。</summary>
    Task<string?> GetTextAsync();

    /// <summary>クリップボードにテキストを設定します。</summary>
    void SetText(string text);

    /// <summary>クリップボードの内容をフラッシュします。</summary>
    void Flush();
}
