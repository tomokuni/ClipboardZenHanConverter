using EsUtil.ZenHanConverter.App.WinForms.ViewModels;
using EsUtil.ZenHanConverter.App.WinForms.Views;
using EsUtil.ZenHanConverter.Core.Interfaces;
using EsUtil.ZenHanConverter.Core.Logic;
using EsUtil.ZenHanConverter.Core.Models;
using EsUtil.ZenHanConverter.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace EsUtil.ZenHanConverter.App.WinForms.Services;

/// <summary>依存性注入（DI）コンテナへのサービス登録を提供する拡張メソッドクラス。</summary>
/// <remarks>Program での責務を分離し、サービス登録ロジックを専用クラスに抽出します。<br/>
/// 登録されるサービスは全てシングルトンであり、アプリケーションのライフサイクルと一致します。</remarks>
public static class DependencyInjectionExtensions
{
    /// <summary>アプリケーションの全サービスを DI コンテナに登録します。</summary>
    /// <param name="services">サービスコレクション。</param>
    /// <returns>サービスを登録したサービスコレクション。</returns>
    public static IServiceCollection AddZenHanConverterServices(this IServiceCollection services)
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
        services.AddSingleton<MainForm>();
        services.AddSingleton<HomeView>();
        services.AddSingleton<SettingsView>();

        return services;
    }
}
