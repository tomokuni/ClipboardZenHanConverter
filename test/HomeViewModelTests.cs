using Xunit;
using Moq;
using ClipboardZenHanConverter.ViewModels;
using ClipboardZenHanConverter.Core.Models;
using ClipboardZenHanConverter.Core.Logic;
using ClipboardZenHanConverter.Core.Services;
using ClipboardZenHanConverter.Views.Navigation;
using System.Threading.Tasks;
using System;
using ClipboardZenHanConverter.Core.Enums;

namespace ClipboardZenHanConverter.Tests;

public class HomeViewModelTests
{
    [Fact]
    public void Clipboard_ContentChanged_ConvertsAndSetsText()
    {
        // Arrange
        var config = new ConvertConfig { IsEnabledZenHan = true, ConvertModeAlphabet = ZenHanMode.ToHan };
        var converter = new CharConverter(config);
        var mockClipboard = new Mock<IClipboardService>();
        var mockNavigation = new Mock<INavigationService>();
        
        var input = "ＡＢＣ";
        var expected = "ABC";

        mockClipboard.Setup(c => c.GetTextAsync()).ReturnsAsync(input);

        var viewModel = new HomeViewModel(config, converter, mockClipboard.Object, mockNavigation.Object);

        // Act
        // Simulate event trigger
        mockClipboard.Raise(c => c.ContentChanged += null, EventArgs.Empty);

        // Assert
        // Verify SetText was called with converted text
        mockClipboard.Verify(c => c.SetText(expected), Times.Once);
        mockClipboard.Verify(c => c.Flush(), Times.Once);
    }

    [Fact]
    public void Clipboard_ContentChanged_DoesNotSetText_IfNoChange()
    {
        // Arrange
        var config = new ConvertConfig { IsEnabledZenHan = true, ConvertModeAlphabet = ZenHanMode.ToHan };
        var converter = new CharConverter(config);
        var mockClipboard = new Mock<IClipboardService>();
        var mockNavigation = new Mock<INavigationService>();
        
        var input = "ABC"; // Already Han
        
        mockClipboard.Setup(c => c.GetTextAsync()).ReturnsAsync(input);

        var viewModel = new HomeViewModel(config, converter, mockClipboard.Object, mockNavigation.Object);

        // Act
        mockClipboard.Raise(c => c.ContentChanged += null, EventArgs.Empty);

        // Assert
        mockClipboard.Verify(c => c.SetText(It.IsAny<string>()), Times.Never);
    }
}

