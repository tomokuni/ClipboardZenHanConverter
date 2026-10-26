using EsUtil.ZenHanConverter.App.WinUI3.Services;
using EsUtil.ZenHanConverter.App.WinUI3.ViewModels;
using Xunit;

namespace EsUtil.ZenHanConverter.Tests.App.WinUI3.ViewModels;

/// <summary>MainWindowViewModelTests の検証項目を表します。</summary>
public class MainWindowViewModelTests
{
    [Fact]
    internal void Constructor_SelectedPageはnull()
    {
        var vm = new MainWindowViewModel(new TestNavigationService());
        Assert.Null(vm.SelectedPage);
    }

    [Fact]
    internal void SelectedPage_変更でナビゲーションが呼ばれる()
    {
        var nav = new TestNavigationService();
        _ = new MainWindowViewModel(nav)
        {
            SelectedPage = "Settings"
        };
        Assert.Equal("Settings", nav.LastPage);
    }

    private sealed class TestNavigationService : INavigationService
    {
        public object? LastPage { get; private set; }

        public void Initialize() { }
        public void NavigateTo(object? page) => LastPage = page;
        public void PreloadSettings() { }
    }
}
