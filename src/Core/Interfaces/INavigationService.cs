namespace ClipboardZenHanConverter.Core.Interfaces;

/// <summary>ページ間遷移を管理するサービスです。</summary>
public interface INavigationService
{
    /// <summary>指定されたページへ遷移します。</summary>
    /// <param name="selectedPage">遷移対象のページ情報（NavigationViewItem または型名文字列）</param>
    void NavigateTo(object? selectedPage);
}
