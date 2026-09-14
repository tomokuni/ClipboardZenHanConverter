# ClipboardZenHanConverter.Presentation 外部機能仕様書

本ドキュメントは `src/core_presentation`（UI フレームワーク非依存のプレゼンテーション層）の公開 API 仕様を記載します。
メソッド内部のコードは記載しません。外部利用者に関係する情報のみを扱います。

## 共有コア

モデル・変換ロジック・Win32 API・アイコンデータ・変換カテゴリ定義は `src/core`（UI 非依存）が提供します。
公開 API は `../core/SPEC_ExtFunc.md` を参照してください。

## 公開型一覧

### ViewModels（`EsUtil.ClipboardZenHanConverter.Presentation.ViewModels`）

4 つの UI（MewUI 版 / WinUI 3 版 / Avalonia UI 版 / WinForms 版）が共有します。

#### `SettingsViewModel : ObservableObject`

設定画面のデータ管理。

- `ConvertConfig ConvertConfig` — 変換設定
- `IList<ZenHanConvertItem> NumberItems` / `AlphabetItems` / `KanaItems` / `SymbolItems` — 各カテゴリの変換アイテム一覧
- `IList<ZenHanConvertItem> EtcZenHanAsciiItems` / `EtcBslashYenItems` / `EtcSpecialItems` / `EtcMultiSpaceItems` — その他の変換アイテム一覧
- `ObservableCollection<string> PresetNames` — プリセット名一覧
- `ObservableCollection<ReplacePairItem> ReplaceItems` — 置換ルール一覧
- `string? SelectedPresetName` — 選択中のプリセット名（変更時にプリセットを読み込む）
- `void ValidateAllAndSyncToConfig()` — 全置換行を検証し、有効な行のみを設定へ反映
- `void ValidateItemAndSync(ReplacePairItem item)` — 指定行を検証して全体を反映
- `void ReloadReplaceItemsFromConfig()` — 置換ルール一覧を設定から再構築
- `void RefreshPresets()` — プリセット一覧を更新
- `void ExportSettings(string filePath)` — 設定のエクスポート
- `string? ImportSettings(string filePath)` — 設定のインポート（成功時 null / 失敗時メッセージ）
- `void SavePreset(string name)` — プリセットの保存（空・空白のみは何もしない）
- `void LoadPreset(string? name)` — プリセットの読み込み（null・空は何もしない）
- `void DeletePreset(string? name)` — プリセットの削除（null・空・組込みプリセットは何もしない）
- `IRelayCommand AddReplaceRowCommand` — 置換行の追加
- `IRelayCommand DeleteReplaceRowCommand` — 置換行の削除

置換ルールの編集は `ReplacePairItem` の変更通知を購読して自動で検証・反映するため、
UI 側から `ValidateItemAndSync` を呼ぶ必要はありません（公開しているのは再同期を明示したい場合のためです）。

#### `ZenHanConvertItem : ObservableObject, IDisposable`

設定画面の変換項目（1 つの変換設定に対応）。

- `string Label` — 項目ラベル
- `bool IsEnabled` — 有効状態
- `bool? ForceEnableState` — 強制的な有効状態（指定がない場合は null）
- `IReadOnlyList<SegmentOption> Options` — 選択肢ごとにコントロールを並べる UI 用の一覧
- `string SelectedLabel` — 選択肢を 1 つのコントロールで表す UI 用の選択値（変更時は設定へ反映）
- `void Dispose()` — イベント購読の解除（重複呼び出しは安全）

#### `SegmentOption : ObservableObject`

排他選択の選択肢 1 件。

- `string Content` — 表示テキスト
- `string GroupName` — 排他選択のグループ名（同一行内で同一・行間で異なる）
- `bool IsEnabled` — この選択肢が有効かどうか
- `bool IsSelected` — 選択状態（`ZenHanConvertItem` が設定と同期する）

#### `ReplacePairItem : ObservableObject`

文字列置換ペアの編集項目。

- `string Search` — 検索文字列（空不可）
- `string Replace` — 置換文字列（空不可）
- `bool IsRegex` — 正規表現を使用するかどうか
- `string? ErrorMessage` — バリデーションエラーメッセージ（null ならエラーなし）
- `string? Validate()` — バリデーションの実行（正常時は null）
- `ReplacePair ToPair()` — `ReplacePair` への変換（事前に `Validate` の呼び出しが必要）

#### `PresetEditDialogViewModel : ObservableObject`

プリセット編集ダイアログのデータと検証。

- `string PresetName` — 入力されたプリセット名（変更時に検証）
- `string? ErrorMessage` — 検証エラーメッセージ
- `bool CanSave` / `bool CanDelete` — 各操作の可否
- `void SetCloseAction(Action closeAction)` — ダイアログを閉じるアクションの登録
- `IRelayCommand SaveCommand` / `DeleteCommand` / `CloseCommand`
- `internal static bool ContainsInvalidFileNameChars(string name)` — 使用不可文字の判定
- `internal static bool ContainsBuiltInKeyword(string name)` — "built-in" / "builtin" の判定

検証結果の組み合わせは次のとおりです。

| 条件 | メッセージ | 保存 | 削除 |
| --- | --- | --- | --- |
| 空文字 | なし | 無効 | 無効 |
| 組込みプリセット | 組込みプリセットです。 | 無効 | 無効 |
| 使用不可文字を含む | 使用できない文字が含まれます。 | 無効 | 既存なら有効 |
| built-in / builtin を含む | built-in または builtin は使用できません。 | 無効 | 既存なら有効 |
| 既存ユーザープリセット | 既に存在します。 | 有効 | 有効 |
| 新規の有効な名前 | なし | 有効 | 無効 |

#### `HomeDisplayText`（静的クラス）

ホーム画面が共有する表示文言の単一所有元。

- `const string NoChange` — 変換結果が元テキストと同一の場合に表示する文言（「変換不要 (変更なし)」）

4 つの UI の `HomeViewModel` がこの定数を参照します。テストは仕様の固定のため、定数ではなくリテラルで期待値を記述します。
