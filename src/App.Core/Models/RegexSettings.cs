using System;

namespace EsUtil.ZenHanConverter.App.Core.Models;

/// <summary>正規表現の共通設定を単一所有します。</summary>
/// <remarks>ユーザー定義の置換ルールは正規表現を受け付けるため、**暴走を防ぐタイムアウト**を必ず付けます
/// （MA0009。 入力は利用者が作るため、破滅的なバックトラックを招くパターンが書けます）。</remarks>
public static class RegexSettings
{
    /// <summary>正規表現の一致判定に許す時間。</summary>
    /// <remarks>通常の置換は 1 ミリ秒未満で終わるため、1 秒は「明らかな暴走」だけを止める値です。</remarks>
    public static TimeSpan MatchTimeout { get; } = TimeSpan.FromSeconds(1);
}
