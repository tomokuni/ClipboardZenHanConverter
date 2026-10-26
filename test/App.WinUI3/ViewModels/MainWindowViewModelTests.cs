using EsUtil.ZenHanConverter.App.WinUI3.Services;
using EsUtil.ZenHanConverter.App.WinUI3.ViewModels;
using Xunit;

namespace EsUtil.ZenHanConverter.Tests.App.WinUI3.ViewModels;

public class MainWindowViewModelTests
{
    [Fact]
    public void Constructor_SelectedPageはnull()
    {
        var vm = new MainWindowViewModel(new TestNavigationService());
        Assert.Null(vm.SelectedPage);
    }

    [Fact]
    public void SelectedPage_変更でナビゲーションが呼ばれる()
    {
        var nav = new TestNavigationService();
        var vm = new MainWindowViewModel(nav);
        vm.SelectedPage = "Settings";
        Assert.Equal("Settings", nav.LastPage);
    }

    private sealed class TestNavigationService : INavigationService
    {
        public object? LastPage { get; private set; }

        public void Initialize() { }
        public void NavigateTo(object? page) => LastPage = page;
        public void PreloadSettingsAsync() { }
    }
}
