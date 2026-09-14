# ClipboardZenHanConverter.Presentation

4 つの UI 実装（MewUI 版 / WinUI 3 版 / Avalonia UI 版 / WinForms 版）が共有する**UI フレームワーク非依存のプレゼンテーション層**です。

## 目的

設定画面の ViewModel は、共有コア（`src/core`）と `CommunityToolkit.Mvvm` だけに依存し、
UI フレームワークの型を一切参照しません。それにもかかわらず、共通化前は 4 つのアプリが
それぞれ同じ実装を持っていました（2 系統に分かれた完全な重複）。

本プロジェクトへ 1 つに集約したことで、次を解消しています。

- 同じコードを 4 か所で保守する負担（合計 2132 行 → 677 行）
- 片方だけを修正して**変換結果や設定仕様がアプリごとにずれる**リスク

## 収録する ViewModel

| 型 | 責務 |
| --- | --- |
| `SettingsViewModel` | 設定画面のデータ管理（8 カテゴリの変換設定・置換ルール・プリセット・JSON 入出力） |
| `ZenHanConvertItem` | 変換項目 1 件（ラベル・有効状態・排他選択） |
| `SegmentOption` | 排他選択の選択肢 1 件（表示テキスト・選択状態） |
| `ReplacePairItem` | 置換ルール 1 行（検索文字列・置換文字列・正規表現・検証エラー） |
| `PresetEditDialogViewModel` | プリセット編集ダイアログ（名前の検証・保存・削除） |
| `HomeDisplayText` | ホーム画面が共有する表示文言（「変換不要 (変更なし)」） |

`HomeViewModel` は UI スレッドへの委譲方法がフレームワークごとに異なるため、各アプリに残しています。

## 排他選択の 2 通りの使い方

`ZenHanConvertItem` は、UI フレームワークごとに異なる排他選択の表現に対応するため、次の 2 つの窓口を持ちます。

| 窓口 | 用途 | 使用している UI |
| --- | --- | --- |
| `Options`（`SegmentOption` の一覧） | 選択肢ごとにコントロールを並べる UI へ、`IsSelected` を双方向バインドする | Avalonia UI 版・WinForms 版 |
| `SelectedLabel`（文字列） | 選択肢を 1 つのコントロールで表す UI へ、選択値として双方向バインドする | MewUI 版・WinUI 3 版 |

どちらの経路から変更しても設定へ反映され、設定側からの変更は双方へ再同期されます。

## 依存方向

```text
App（app_MewUI / app_WinUI3 / app_AvaloniaUI / app_WinForms）
  └──> Presentation（本プロジェクト）
         └──> Core（モデル・変換ロジック・Win32 API）
```

本プロジェクトが UI フレームワークへ依存することはありません。逆方向の依存もありません。

## ビルド

```powershell
dotnet build src/core_presentation/core_presentation.csproj
```

## テスト

`test/core_presentation` が本プロジェクトの単体テストを持ちます。

```powershell
dotnet test test/core_presentation/tests_core_presentation.csproj
```

詳細な設計は `SPEC_System.md`、公開 API は `SPEC_ExtFunc.md` を参照してください。
