namespace ClipboardZenHanConverter.Views.Navigation
{
    /// <summary>ナビゲーションサービスで指定されたページに移動するためのインターフェースです。</summary>
    /// <remarks>
    /// このインターフェースの役割は、通常、UIフレームワークでのページ間遷移を管理します。<br/>
    /// </remarks>
    public interface INavigationService
    {
        /// <summary>指定されたページ情報に基づき、対応するページ型へ遷移します。</summary>
        /// <param name="selectedPage">遷移対象のページ情報（NavigationViewItem または型名文字列）</param>
        void NavigateTo(object? selectedPage);
    }
}
