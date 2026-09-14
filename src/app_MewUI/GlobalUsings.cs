// 共有プレゼンテーション層（UI 非依存の ViewModel）を各画面から修飾なしで参照する。
// 依存方向: 本アプリ → ClipboardZenHanConverter.Presentation → ClipboardZenHanConverter.Core
// これにより 4 つの UI が同じ ViewModel 実装（設定画面・置換ルール・プリセット編集）を共有する。
global using ClipboardZenHanConverter.Presentation.ViewModels;
