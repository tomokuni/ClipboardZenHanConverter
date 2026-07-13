using System.Text.Json.Serialization;

namespace ClipboardZenHanConverter.Core.Models;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified)]
[JsonSerializable(typeof(AppSetting))]
[JsonSerializable(typeof(ConvertConfig))]
[JsonSerializable(typeof(ReplacePair))]
internal partial class AppJsonContext : JsonSerializerContext
{
}
