using System.Windows.Forms;

namespace EsUtil.ClipboardZenHanConverter.App.WinForms.Services;

/// <summary>アプリケーション内の画面遷移を管理する抽象（DIP）。</summary>
/// <remarks><see cref="Control"/> を扱うため、共有コアではなくアプリ層に配置しています。</remarks>
public interface INavigationService
{
    /// <summary>タグに対応するビューを解決します。</summary>
    /// <param name="tag">ページタグ（"Home" / "Settings"）。</param>
    /// <returns>タグに対応するビュー。同一タグに対しては同じインスタンスを返します。</returns>
    Control ResolveView(string tag);
}
