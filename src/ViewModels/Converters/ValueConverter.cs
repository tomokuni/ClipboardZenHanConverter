using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml.Data;
using System;

using ClipboardZenHanConverter.Core.Enums;
using ClipboardZenHanConverter.Core.Models;

namespace ClipboardZenHanConverter.ViewModels.Converters;



public partial class TextToZenHanModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // ZenHanMode → ItemValue
        return (value as ZenHanMode?) switch
        {
            ZenHanMode.ToZen => SettingsModel.TextToZen,
            ZenHanMode.ToHan => SettingsModel.TextToHan,
            _ => SettingsModel.TextNone
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // ItemValue → ZenHanMode

        string? text = value switch
        {
            string s => s,
            SegmentedItem item => item.Content as string,
            _ => null
        };

        return text switch
        {
            SettingsModel.TextToZen => ZenHanMode.ToZen,
            SettingsModel.TextToHan => ZenHanMode.ToHan,
            _ => ZenHanMode.None,
        };
    }
}

public partial class TextToZenHanKanaModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // ZenHanKanaMode → ItemValue
        return (value as ZenHanKanaMode?) switch
        {
            ZenHanKanaMode.ToHan => SettingsModel.TextKanaToHan,
            ZenHanKanaMode.ToZenKata => SettingsModel.TextKanaToZenKata,
            ZenHanKanaMode.ToZenHira => SettingsModel.TextKanaToZenHira,
            _ => SettingsModel.TextKanaNone
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // ItemValue → ZenHanKanaMode
        string? text = value switch
        {
            string s => s,
            SegmentedItem item => item.Content as string,
            _ => null
        };

        return text switch
        {
            SettingsModel.TextKanaToHan => ZenHanKanaMode.ToHan,
            SettingsModel.TextKanaToZenKata => ZenHanKanaMode.ToZenKata,
            SettingsModel.TextKanaToZenHira => ZenHanKanaMode.ToZenHira,
            _ => ZenHanKanaMode.None,
        };
    }
}
