namespace ClipboardZenHanConverter.Core.Interfaces;

/// <summary>アプリケーション内のページ遷移を管理するインターフェース。</summary>
/// <remarks>DIP に従い、ViewModels はこのインターフェースを通じてナビゲーションを行います。<br/>
/// 実装クラスはプラットフォーム固有のページ遷移ロジックをカプセル化します。</remarks>
public interface INavigationService
{
    /// <summary>指定されたページへ遷移します。</summary>
    /// <param name="page">遷移先を示すオブジェクト（文字列、NavigationViewItem 等）</param>
    void NavigateTo(object? page);

    /// <summary>ナビゲーションサービスを初期化します。</summary>
    void Initialize();

    /// <summary>SettingsPage をバックグラウンドで事前生成します。</summary>
    /// <remarks>UIスレッドがアイドルになったタイミングで設定画面のXAML解析と生成を実行します。</remarks>
    void PreloadSettingsAsync();
}
