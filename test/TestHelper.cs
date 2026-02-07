using ClipboardZenHanConverter.Core.Models;
using ClipboardZenHanConverter.Core.Enums;

namespace ClipboardZenHanConverter.Tests;

public static class TestHelper
{
    public static ConvertConfig CreateDefaultConfig()
    {
        return new ConvertConfig
        {
            IsEnabledZenHan = true,
        };
    }
}
