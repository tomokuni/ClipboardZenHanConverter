using Xunit;
using ClipboardZenHanConverter.App.Helpers;
using ClipboardZenHanConverter.App.ViewModels;
using ClipboardZenHanConverter.Core.Models;
using ClipboardZenHanConverter.Core.Enums;

namespace ClipboardZenHanConverter.Tests;

public class SettingsViewModelTests
{
    [Fact]
    public void Constructor_InitializesItems()
    {
        // Arrange
        var config = new ConvertConfig();

        // Act
        var vm = new SettingsViewModel(config);

        // Assert
        Assert.NotEmpty(vm.NumberItems);
        Assert.NotEmpty(vm.AlphabetItems);
        Assert.Equal(SegmentDefinitions.NumberDefs.Length, vm.NumberItems.Count);
        Assert.Equal(SegmentDefinitions.AlphabetDefs.Length, vm.AlphabetItems.Count);
    }

    [Fact]
    public void Item_SelectionChange_UpdatesConfig()
    {
        // Arrange
        var config = new ConvertConfig { ConvertModeNumber = ZenHanMode.None };
        var vm = new SettingsViewModel(config);

        var numberItem = vm.NumberItems.First(); // Def: "数字 の変換"
        // Ensure we pick a segment that is different from current val (None)
        var toHanSegment = numberItem.Segments.First(s => s.Content == "半角");

        // Act
        numberItem.SelectedLabel = toHanSegment.Content;

        // Assert
        Assert.Equal(ZenHanMode.ToHan, config.ConvertModeNumber);
    }

    [Fact]
    public void Item_SelectedLabel_ReflectsConfigChange()
    {
        // Arrange
        var config = new ConvertConfig { ConvertModeNumber = ZenHanMode.None };
        var vm = new SettingsViewModel(config);
        var numberItem = vm.NumberItems.First();

        // Act
        config.ConvertModeNumber = ZenHanMode.ToZen;

        // Assert
        Assert.Equal("全角", numberItem.SelectedLabel);
    }
}
