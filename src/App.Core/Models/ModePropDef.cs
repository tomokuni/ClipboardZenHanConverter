using System;

namespace EsUtil.ZenHanConverter.App.Core.Models;

/// <summary>モードプロパティの単一定義。</summary>
/// <remarks>Get/Set デリゲートのペアで、<see cref="ConvertConfig"/> のモードプロパティを型安全にアクセスします。<br/>
/// <c>ToZenValue</c> に値が設定されている場合、そのプロパティは「全力会計」プリセットで全角設定の対象となります。<br/>
/// UI 側はこの定義を直接参照することで、プロパティ名文字列による実行時名前解決を避けます（AOT 互換）。</remarks>
/// <param name="Get">プロパティの値を取得するデリゲート。</param>
/// <param name="Set">プロパティへ値を設定するデリゲート。</param>
/// <param name="ToZenValue">全角設定で使う値。null の場合は全角設定の対象外。</param>
public sealed record ModePropDef(
    Func<ConvertConfig, object> Get,
    Action<ConvertConfig, object> Set,
    object? ToZenValue);
