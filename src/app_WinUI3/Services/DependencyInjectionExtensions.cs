using EsUtil.ClipboardZenHanConverter.App.WinUI.ViewModels;
using EsUtil.ClipboardZenHanConverter.App.WinUI.Views;
using EsUtil.ClipboardZenHanConverter.Core.Interfaces;
using EsUtil.ClipboardZenHanConverter.Core.Logic;
using EsUtil.ClipboardZenHanConverter.Core.Models;
using EsUtil.ClipboardZenHanConverter.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<SettingsViewModel>();

        // Views
        services.AddSingleton<MainWindow>();
        services.AddSingleton<HomePage>();
        services.AddSingleton<SettingsPage>();

        return services;
    }
}
