using System.Text.Json.Serialization;

namespace ClipboardZenHanConverter.Core.Models;

/// <summary>System.Text.Json のソースジェネレーター対応JSONシリアライゼーションコンテキスト。</summary>
/// <remarks>AppSetting、ConvertConfig、ReplacePair の高速なシリアライズ/デシリアライズを提供します。<br/>
/// WriteIndented: true（見やすいインデント付きJSON出力）<br/>
/// DefaultIgnoreCondition: WhenWritingNull（null値を出力しない）<br/>
/// PropertyNamingPolicy: Unspecified（プロパティ名をそのまま使用）</remarks>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified)]
[JsonSerializable(typeof(AppSetting))]
[JsonSerializable(typeof(ConvertConfig))]
[JsonSerializable(typeof(ReplacePair))]
internal partial class AppJsonContext : JsonSerializerContext;
