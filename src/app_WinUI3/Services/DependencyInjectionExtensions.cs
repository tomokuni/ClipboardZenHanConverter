using EsUtil.ClipboardZenHanConverter.App.WinUI.ViewModels;
using EsUtil.ClipboardZenHanConverter.App.WinUI.Views;
using EsUtil.ClipboardZenHanConverter.Core.Interfaces;
using EsUtil.ClipboardZenHanConverter.Core.Logic;
using EsUtil.ClipboardZenHanConverter.Core.Models;
using EsUtil.ClipboardZenHanConverter.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;

namespace EsUtil.ClipboardZenHanConverter.App.WinUI.Services;

/// <summary>依存性注入（DI）コンテナへのサービス登録を提供する拡張メソッドクラス。</summary>
/// <remarks>App.xaml.cs での責務を分離し、サービス登録ロジックを専用クラスに抽出します。<br/>
/// 登録されるサービスは全てシングルトンであり、アプリケーションのライフサイクルと一致します。</remarks>
public static class DependencyInjectionExtensions
{
    /// <summary>アプリケーションの全サービスを DI コンテナに登録します。</summary>
    /// <param name="services">サービスコレクション。</param>
    /// <remarks>登録対象: ナビゲーションサービス、クリップボードサービス、コアロジック（CharConverter）、
    /// モデル（AppSetting、ConvertConfig）、ViewModels（MainWindow/Home/Settings）、Views（MainWindow/HomePage/SettingsPage）</remarks>
    public static IServiceCollection AddClipboardZenHanConverterServices(this IServiceCollection services)
    {
        // サービス
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IClipboardService, ClipboardService>();

        // コアロジック
        services.AddSingleton<ITextConverter, CharConverter>();

        // モデル
        services.AddSingleton<AppSetting>();
        services.AddSingleton<ConvertConfig>();

        // ViewModels
        services.AddSingleton<MainWindowViewModel>();
        // HomeViewModel は UI スレッドへの委譲先を受け取る（UI 層で生成して渡す）
        services.AddSingleton(sp => new HomeViewModel(
            sp.GetRequiredService<ITextConverter>(),
            sp.GetRequiredService<IClipboardService>(),
            sp.GetRequiredService<AppSetting>(),
            CreateUiDispatcher()));
        services.AddSingleton<SettingsViewModel>();

        // Views
        services.AddSingleton<MainWindow>();
        services.AddSingleton<HomePage>();
        services.AddSingleton<SettingsPage>();

        return services;
    }

    /// <summary>現在のスレッドのディスパッチキューをラップした UI 委譲先を生成します。</summary>
    /// <returns>UI 委譲先。UI スレッド以外（ディスパッチキューが取得できない場合）は null。</returns>
    /// <remarks>DI の解決は UI スレッドで行われるため、ここで取得したキューが UI スレッドのものになります。</remarks>
    private static IUiDispatcher? CreateUiDispatcher()
    {
        var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        return dispatcherQueue is null ? null : new DispatcherQueueUiDispatcher(dispatcherQueue);
    }
}
