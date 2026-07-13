namespace ClipboardZenHanConverter.Core.Interfaces;

/// <summary>ページの遷移ライフサイクルを通知するインターフェースです。</summary>
public interface INavigationAware
{
    /// <summary>このページに遷移してきたときに呼び出されます。</summary>
    /// <param name="parameter">ナビゲーションパラメータ</param>
    void OnNavigatedTo(object? parameter);

    /// <summary>このページから別のページに遷移するときに呼び出されます。</summary>
    void OnNavigatingFrom();
}
