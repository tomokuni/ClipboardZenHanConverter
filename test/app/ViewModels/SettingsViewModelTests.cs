using ClipboardZenHanConverter.App.Helpers;
using ClipboardZenHanConverter.App.ViewModels;
using ClipboardZenHanConverter.Core.Enums;
using ClipboardZenHanConverter.Core.Models;
using Xunit;

namespace ClipboardZenHanConverter.Tests.App.ViewModels;

public class SettingsViewModelTests
{
    [Fact]
    public void Constructor_InitializesItems()
    {
        var config = new ConvertConfig();

        var vm = new SettingsViewModel(config);

        Assert.NotEmpty(vm.NumberItems);
        Assert.NotEmpty(vm.AlphabetItems);
        Assert.Equal(SegmentDefinitions.NumberDefs.Length, vm.NumberItems.Count);
        Assert.Equal(SegmentDefinitions.AlphabetDefs.Length, vm.AlphabetItems.Count);
    }

    [Fact]
    public void Item_SelectionChange_UpdatesConfig()
    {
        var config = new ConvertConfig { ConvertModeNumber = ZenHanMode.None };
        var vm = new SettingsViewModel(config);

        var numberItem = vm.NumberItems.First();
        var toHanSegment = numberItem.Segments.First(s => s.Content == "半角");

        numberItem.SelectedLabel = toHanSegment.Content;

        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeNumber);
    }

    [Fact]
    public void Item_SelectedLabel_ReflectsConfigChange()
    {
        var config = new ConvertConfig { ConvertModeNumber = ZenHanMode.None };
        var vm = new SettingsViewModel(config);
        var numberItem = vm.NumberItems.First();

        config.ConvertModeNumber = ZenHanMode.ToZen;

        Assert.Equal("全角", numberItem.SelectedLabel);
    }
}
