using Xunit;
using ClipboardZenHanConverter.ViewModels;
using ClipboardZenHanConverter.Core.Models;
using ClipboardZenHanConverter.Core.Enums;
using System.Linq;

namespace ClipboardZenHanConverter.Tests;

public class SettingsViewModelTests
{
    [Fact]
    public void Constructor_InitializesItems()
    {
        // Arrange
        var config = new ConvertConfig();
        var model = new SettingsModel();

        // Act
        var vm = new SettingsViewModel(config, model);

        // Assert
        Assert.NotEmpty(vm.NumberItems);
        Assert.NotEmpty(vm.AlphabetItems);
        Assert.Equal(model.NumberDefs.Length, vm.NumberItems.Count);
        Assert.Equal(model.AlphabetDefs.Length, vm.AlphabetItems.Count);
    }

    [Fact]
    public void Item_SelectionChange_UpdatesConfig()
    {
        // Arrange
        var config = new ConvertConfig { ConvertModeNumber = ZenHanMode.None };
        var model = new SettingsModel();
        var vm = new SettingsViewModel(config, model);
        
        var numberItem = vm.NumberItems.First(); // Def: "数字 の変換"
        // Ensure we pick a segment that is different from current val (None)
        var toHanSegment = numberItem.Segments.First(s => s.Content == SettingsModel.TextToHan);

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
        var model = new SettingsModel();
        var vm = new SettingsViewModel(config, model);
        var numberItem = vm.NumberItems.First();

        // Act
        config.ConvertModeNumber = ZenHanMode.ToZen;

        // Assert
        Assert.Equal(SettingsModel.TextToZen, numberItem.SelectedLabel);
    }
}
