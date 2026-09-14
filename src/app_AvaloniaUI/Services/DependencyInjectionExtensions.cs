using EsUtil.ClipboardZenHanConverter.App.AvaloniaUI.ViewModels;
using EsUtil.ClipboardZenHanConverter.App.AvaloniaUI.Views;
using EsUtil.ClipboardZenHanConverter.Core.Interfaces;
using EsUtil.ClipboardZenHanConverter.Core.Logic;
using EsUtil.ClipboardZenHanConverter.Core.Models;
using EsUtil.ClipboardZenHanConverter.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace EsUtil.ClipboardZenHanConverter.App.AvaloniaUI.Services;

/// <summary>依存性注入（DI）コンテナへのサービス登録を提供する拡張メソッドクラス。</summary>
/// <remarks>App での責務を分離し、サービス登録ロジックを専用クラスに抽出します。<br/>
/// 登録されるサービスは全てシングルトンであり、アプリケーションのライフサイクルと一致します。</remarks>
public static class DependencyInjectionExtensions
{
    /// <summary>アプリケーションの全サービスを DI コンテナに登録します。</summary>
    /// <param name="services">サービスコレクション。</param>
    /// <returns>サービスを登録したサービスコレクション。</returns>
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
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<SettingsViewModel>();

        // Views
        services.AddSingleton<MainWindow>();
        services.AddSingleton<HomeView>();
        services.AddSingleton<SettingsView>();

        return services;
    }
}
