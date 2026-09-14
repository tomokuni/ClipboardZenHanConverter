using Avalonia.Controls;

namespace ClipboardZenHanConverter.App.AvaloniaUI.Services;

/// <summary>アプリケーション内の画面遷移を管理する抽象（DIP）。</summary>
/// <remarks><see cref="Control"/> を扱うため、共有コアではなくアプリ層に配置しています。</remarks>
public interface INavigationService
{
    /// <summary>タグに対応するビューを解決します。</summary>
    /// <param name="tag">ページタグ（"Home" / "Settings"）。</param>
    /// <returns>タグに対応するビュー。同一タグに対しては同じインスタンスを返します。</returns>
    Control ResolveView(string tag);

    /// <summary>指定されたタグのビューを事前生成します。</summary>
    /// <param name="tag">ページタグ（"Home" / "Settings"）。</param>
    /// <remarks>UI スレッドがアイドル状態のタイミングで生成し、初回遷移のコストを削減します。</remarks>
    void PreloadView(string tag);
}
