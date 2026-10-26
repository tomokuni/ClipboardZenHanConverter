using System;
using System.Text.Json.Serialization;

namespace EsUtil.ZenHanConverter.App.Core.Models;

/// <summary>シリアライズに使用する <see cref="JsonSerializerContext"/> と型を保持するレコード。</summary>
/// <param name="Context">ソース生成された <see cref="JsonSerializerContext"/>。</param>
/// <param name="Type">シリアライズ対象の型。</param>
public sealed record SerializableTypeInfo(JsonSerializerContext Context, Type Type);
