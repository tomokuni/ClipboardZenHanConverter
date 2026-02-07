using System;
using System.Collections.Generic;
using System.Linq;
using ClipboardZenHanConverter.Core.Enums;
using ClipboardZenHanConverter.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardZenHanConverter.ViewModels;

/// <summary>設定画面のデータを管理するViewModelクラスです。</summary>
/// <remarks>
/// 設定関連のデータを管理します。<br/>
/// </remarks>
public partial class SettingsViewModel(ConvertConfig convertConfig, SettingsModel model) : ObservableObject
{
    public ConvertConfig ConvertConfig { get; } = convertConfig;

    public IList<ZenHanConvertItem> NumberItems { get; } = [.. model.NumberDefs.Select(d => new ZenHanConvertItem(convertConfig, d))];
    public IList<ZenHanConvertItem> AlphabetItems { get; } = [.. model.AlphabetDefs.Select(d => new ZenHanConvertItem(convertConfig, d))];
    public IList<ZenHanConvertItem> KanaItems { get; } = [.. model.KanaDefs.Select(d => new ZenHanConvertItem(convertConfig, d))];
    public IList<ZenHanConvertItem> SymbolItems { get; } = [.. model.SymbolDefs.Select(d => new ZenHanConvertItem(convertConfig, d))];
    public IList<ZenHanConvertItem> EtcZenHanAsciiItems { get; } = [.. model.EtcZenHanAsciiDefs.Select(d => new ZenHanConvertItem(convertConfig, d))];
    public IList<ZenHanConvertItem> EtcBslashYenItems { get; } = [.. model.EtcBslashYenDefs.Select(d => new ZenHanConvertItem(convertConfig, d))];
    public IList<ZenHanConvertItem> EtcSpecialItems { get; } = [.. model.EtcSpecialDefs.Select(d => new ZenHanConvertItem(convertConfig, d))];
    public IList<ZenHanConvertItem> EtcMultiSpaceItems { get; } = [.. model.EtcMultiSpaceDefs.Select(d => new ZenHanConvertItem(convertConfig, d))];
}

