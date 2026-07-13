namespace ClipboardZenHanConverter.Core.Models;

/// <summary>ユーザー定義の文字列置換ペアを表します。</summary>
/// <param name="Search">置換元の文字列（正規表現可）</param>
/// <param name="Replace">置換先の文字列</param>
/// <param name="IsRegex">正規表現として扱う場合は true</param>
public record ReplacePair(string Search, string Replace, bool IsRegex = false);
