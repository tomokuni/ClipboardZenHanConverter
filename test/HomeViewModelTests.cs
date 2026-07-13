using Xunit;
using Moq;
using ClipboardZenHanConverter.ViewModels;
using ClipboardZenHanConverter.Core.Models;
using ClipboardZenHanConverter.Core.Logic;
using ClipboardZenHanConverter.Core.Interfaces;
using System;
using ClipboardZenHanConverter.Core.Enums;

namespace ClipboardZenHanConverter.Tests;

public class HomeViewModelTests
{
    [Fact]
    public async Task Clipboard_ContentChanged_ConvertsAndSetsText()
    {
        var config = new ConvertConfig { IsEnabledZenHan = true, ConvertModeAlphabet = ZenHanMode.ToHan };
        var converter = new CharConverter(config);
        var mockClipboard = new Mock<IClipboardService>();
        var mockNavigation = new Mock<INavigationService>();

        mockClipboard.Setup(c => c.GetTextAsync()).ReturnsAsync("ＡＢＣ");

        var viewModel = new HomeViewModel(config, converter, mockClipboard.Object, mockNavigation.Object);
        viewModel.TestMode = true;

        mockClipboard.Raise(c => c.ContentChanged += null, EventArgs.Empty);

        mockClipboard.Verify(c => c.SetText("ABC"), Times.Once);
        mockClipboard.Verify(c => c.Flush(), Times.Once);
    }

    [Fact]
    public void Clipboard_ContentChanged_DoesNotSetText_IfNoChange()
    {
        var config = new ConvertConfig { IsEnabledZenHan = true, ConvertModeAlphabet = ZenHanMode.ToHan };
        var converter = new CharConverter(config);
        var mockClipboard = new Mock<IClipboardService>();
        var mockNavigation = new Mock<INavigationService>();

        mockClipboard.Setup(c => c.GetTextAsync()).ReturnsAsync("ABC");

        var viewModel = new HomeViewModel(config, converter, mockClipboard.Object, mockNavigation.Object);
        viewModel.TestMode = true;

        mockClipboard.Raise(c => c.ContentChanged += null, EventArgs.Empty);

        mockClipboard.Verify(c => c.SetText(It.IsAny<string>()), Times.Never);
    }
}

