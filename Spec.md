# ClipboardZenHanConverter プログラム仕様書

## プロジェクト構成

```text
ClipboardZenHanConverter/
├── README.md                         # ユーザー向け利用説明書
├── Spec.md                           # 本仕様書
├── src/
│   ├── app/                          # WinUI3 アプリケーションプロジェクト
│   │   ├── App.xaml / App.xaml.cs     # アプリケーションエントリポイント
│   │   ├── app.manifest               # アプリケーションマニフェスト
│   │   ├── Converters/                # XAML 値コンバーター
│   │   ├── Helpers/                   # UI ヘルパー
│   │   ├── Services/                  # アプリケーションサービス
│   │   ├── ViewModels/                # MVVM ViewModel 層
│   │   └── Views/                     # MVVM View 層 (XAML)
│   └── core/                         # コアロジックプロジェクト
│       ├── Enums/                     # 変換モード列挙型
│       ├── Helpers/                   # 変換ヘルパー
│       ├── Interfaces/                # DI 用インターフェース
│       ├── Logic/                     # 変換ロジック
│       └── Models/                    # データモデル
└── test/                             # 単体テストプロジェクト
    ├── app/                           # app プロジェクト対応テスト
    └── core/                          # core プロジェクト対応テスト
```

---

## src/core プロジェクト

### src/core/Enums/ZenHanMode.cs

#### `ZenHanMode` enum

全角/半角変換の基本方向を指定する列挙型。

| 値 | 名称 | 説明 |
| --- | --- | --- |
| 0 | None | 変換なし |
| 1 | ToHan | 全角文字を半角に変換 |
| 2 | ToZen | 半角文字を全角に変換 |

#### `ZenHanKanaMode` enum

かな文字（半角カナ/全角カタカナ/全角ひらがな）の変換方向を指定する列挙型。

| 値 | 名称 | 説明 |
| --- | --- | --- |
| 0 | None | 変換なし |
| 1 | ToHan | 全角かなを半角カナに変換 |
| 2 | ToZenKata | 半角カナ/全角ひらがなを全角カタカナに変換 |
| 3 | ToZenHira | 半角カナ/全角カタカナを全角ひらがなに変換 |

#### `ZenHanEtcZenHanAsciiMode` enum

かな約物（長音/読点/句点）の変換モードを指定する列挙型。

| 値 | 名称 | 説明 |
| --- | --- | --- |
| 0 | None | 変換なし |
| 1 | ToHan | 全角文字を半角に変換 |
| 2 | ToZen | 半角文字を全角に変換 |
| 3 | ToAscii | 対応するASCII文字に変換（例: ー → -） |

#### `ZenHanEtcYenMode` enum

円記号/バックスラッシュの変換モードを指定する列挙型。

| 値 | 名称 | 説明 |
| --- | --- | --- |
| 0 | None | 変換なし |
| 1 | ToHanBSlash | 半角バックスラッシュ（\）に変換 |
| 2 | ToZenBSlash | 全角バックスラッシュ（＼）に変換 |
| 3 | ToHanYen | 半角円記号（¥）に変換 |
| 4 | ToZenYen | 全角円記号（￥）に変換 |

#### `ZenHanEtcSpecial` enum

特殊文字（タブ/改行/連続スペース）の変換モードを指定する列挙型。

| 値 | 名称 | 説明 |
| --- | --- | --- |
| 0 | None | 変換なし（そのまま保持） |
| 1 | Remove | 該当文字を除去 |
| 2 | ToHanSpace | 半角スペースに変換 |
| 3 | ToZenSpace | 全角スペースに変換 |

---

### src/core/Models/ConvertConfig.cs

#### `ModePropDef` record (public)

モードプロパティの単一定義。Get/Set デリゲートペアと全角設定値をカプセル化。

- **Get**: `Func<ConvertConfig, object>` — プロパティ値の取得
- **Set**: `Action<ConvertConfig, object>` — プロパティ値の設定
- **ToZenValue**: `object?` — 「全力会計」プリセットで全角にする値（null の場合は全角設定対象外。数値・英字は半角を維持するため対象外）

UI（`SegmentDefinitions`）と `SegmentDefine.Mode` がこの定義を直接参照するため public。

#### `ConvertConfig.Mode` static class

全モードプロパティの型安全なアクセサ定義（Canonical Source）。プロパティごとの `ModePropDef` を公開し、
プロパティ名文字列による実行時名前解決を排除します。

- 各モードプロパティの `ModePropDef`（`IsEnabledZenHan` / `Number` / `SymbolParenthesis` など）
- `All` (`ModePropDef[]`, internal) — コピー・比較・全角設定の一括適用に使用する全定義の集合

#### `ConvertConfig` partial class

全角/半角変換の全設定を管理するモデルクラス。

**継承**: `SettingsPersistenceBase<ConvertConfig>`

**ObservableProperty**:

- `IsEnabledZenHan` (`bool`) — 全角/半角変換の有効/無効
- `ConvertModeNumber` (`ZenHanMode`) — 数字の変換モード（デフォルト: None）
- `ConvertModeAlphabet` (`ZenHanMode`) — 英字の変換モード（デフォルト: None）
- `ConvertModeSymbolParenthesis` (`ZenHanMode`) — 丸括弧（）の変換モード
- `ConvertModeSymbolSquareBracket` (`ZenHanMode`) — 角括弧［］の変換モード
- `ConvertModeSymbolCurlyBracket` (`ZenHanMode`) — 波括弧｛｝の変換モード
- `ConvertModeSymbolDoubleQuote` (`ZenHanMode`) — ダブルクォート"の変換モード
- `ConvertModeSymbolSingleQuote` (`ZenHanMode`) — シングルクォート'の変換モード
- `ConvertModeSymbolComma` (`ZenHanMode`) — カンマ，の変換モード
- `ConvertModeSymbolPeriod` (`ZenHanMode`) — ピリオド．の変換モード
- `ConvertModeSymbolColon` (`ZenHanMode`) — コロン：の変換モード
- `ConvertModeSymbolSemicolon` (`ZenHanMode`) — セミコロン；の変換モード
- `ConvertModeSymbolLessThan` (`ZenHanMode`) — 不等号＜の変換モード
- `ConvertModeSymbolEqual` (`ZenHanMode`) — イコール＝の変換モード
- `ConvertModeSymbolGreaterThan` (`ZenHanMode`) — 不等号＞の変換モード
- `ConvertModeSymbolPlus` (`ZenHanMode`) — プラス＋の変換モード
- `ConvertModeSymbolHyphenMinus` (`ZenHanMode`) — マイナス－の変換モード
- `ConvertModeSymbolExclamation` (`ZenHanMode`) — 感嘆符！の変換モード
- `ConvertModeSymbolSharp` (`ZenHanMode`) — シャープ＃の変換モード
- `ConvertModeSymbolDollar` (`ZenHanMode`) — ダラー＄の変換モード
- `ConvertModeSymbolPercent` (`ZenHanMode`) — パーセント％の変換モード
- `ConvertModeSymbolAmpersand` (`ZenHanMode`) — アンパサンド＆の変換モード
- `ConvertModeSymbolAsterisk` (`ZenHanMode`) — アスタリスク＊の変換モード
- `ConvertModeSymbolSlash` (`ZenHanMode`) — スラッシュ／の変換モード
- `ConvertModeSymbolQuestion` (`ZenHanMode`) — 疑問符？の変換モード
- `ConvertModeSymbolAt` (`ZenHanMode`) — アットマーク＠の変換モード
- `ConvertModeSymbolCaret` (`ZenHanMode`) — キャレット＾の変換モード
- `ConvertModeSymbolUnderBar` (`ZenHanMode`) — アンダースコア＿の変換モード
- `ConvertModeSymbolBackquote` (`ZenHanMode`) — バッククォート`の変換モード
- `ConvertModeSymbolVerticalBar` (`ZenHanMode`) — 縦棒｜の変換モード
- `ConvertModeSymbolTilde` (`ZenHanMode`) — チルダ～の変換モード
- `ConvertModeSymbolSpace` (`ZenHanMode`) — スペース␣の変換モード
- `ConvertModeKanaHan` (`ZenHanKanaMode`) — 半角カナの変換モード
- `ConvertModeKanaZenKata` (`ZenHanKanaMode`) — 全角カタカナの変換モード
- `ConvertModeKanaZenHira` (`ZenHanKanaMode`) — 全角ひらがなの変換モード
- `ConvertModeEtcKanaVoice` (`ZenHanMode`) — かな濁点゛の変換モード
- `ConvertModeEtcKanaSemiVoice` (`ZenHanMode`) — かな半濁点゜の変換モード
- `ConvertModeEtcKanaMiddleDot` (`ZenHanMode`) — かな中点・の変換モード
- `ConvertModeEtcKanaLeftCornerBracket` (`ZenHanMode`) — かな左上括弧「の変換モード
- `ConvertModeEtcKanaRightCornerBracket` (`ZenHanMode`) — かな右下括弧」の変換モード
- `ConvertModeEtcKanaProlong` (`ZenHanEtcZenHanAsciiMode`) — かな長音記号ーの変換モード
- `ConvertModeEtcKanaPeriod` (`ZenHanEtcZenHanAsciiMode`) — かな読点。の変換モード
- `ConvertModeEtcKanaComma` (`ZenHanEtcZenHanAsciiMode`) — かな句点、の変換モード
- `ConvertModeEtcBSlashHan` (`ZenHanEtcYenMode`) — バックスラッシュ半角\の変換モード
- `ConvertModeEtcBSlashZen` (`ZenHanEtcYenMode`) — バックスラッシュ全角＼の変換モード
- `ConvertModeEtcYenHan` (`ZenHanEtcYenMode`) — 円記号半角¥の変換モード
- `ConvertModeEtcYenZen` (`ZenHanEtcYenMode`) — 円記号全角￥の変換モード
- `ConvertModeEtcTabSpace` (`ZenHanEtcSpecial`) — タブ文字の変換モード
- `ConvertModeEtcNewline` (`ZenHanEtcSpecial`) — 改行文字の変換モード
- `ConvertModeEtcMultiSpace` (`ZenHanEtcSpecial`) — 連続スペースの変換モード
- `ReplacePairs` (`List<ReplacePair>`) — ユーザー定義の置換ルール一覧

**定数**:

- `BuiltInPresetAccountingPower` = `"全力会計（Built-in）"` — 組み込みプリセット名（全力会計帳票向け）
- `BuiltInPresetAlphanumericHanKanaZen` = `"英数記号半角、かな全角（Built-in）"` — 組み込みプリセット名（英数記号半角/かな全角）

**静的メソッド**:

- `GetPresetNames()` → `string[]` — 利用可能なプリセット名一覧（組込み→保存の順、各グループ内で名前昇順）
- `DeletePreset(string name)` → `void` — ユーザープリセットを削除（組込みは削除不可）
- `IsBuiltInPreset(string name)` → `bool` — 組込みプリセットかどうかの判定

**インスタンスメソッド**:

- `SavePreset(string name)` → `void` — 現在の設定をプリセットとして保存
- `LoadPreset(string name)` → `bool` — プリセットを読み込み
- `ExportToFile(string filePath)` → `void` — 設定を JSON ファイルにエクスポート
- `ImportFromFile(string filePath)` → `bool` — JSON ファイルから設定をインポート
- `FindMatchingPreset()` → `string?` — 現在の設定と一致するプリセット名を検索（GetPresetNames と同じ順序で先頭から確認）

**内部メソッド**:

- `ApplyFrom(ConvertConfig other)` — 他のインスタンスから全設定をコピー
- `PropertiesEqual(ConvertConfig a, ConvertConfig b)` → `bool` — 全プロパティの一致検証

**最適化**:

- `ConvertConfig.Mode` 配列によるメタデータ駆動（DRY: 50プロパティのコピー/比較を一元管理）
- `_copyActions` / `_compareActions` / `_toZenSymbolSetters` の配列によるループ一括処理
- `FrozenDictionary` による組み込みプリセットの高速ルックアップ
- `FindMatchingPreset` は `GetPresetNames()` と同じ順序で検索するため、表示順と一致確認順が完全に同期

---

### src/core/Models/ReplacePair.cs

#### `ReplacePair` record

ユーザー定義の文字列置換ペア。

- **Search**: `string` — 置換元の文字列（正規表現可）
- **Replace**: `string` — 置換先の文字列
- **IsRegex**: `bool` — 正規表現として扱う場合は true

**静的メソッド**:

- `TryValidate(string search, string replace, bool isRegex, out string? errorMessage)` → `bool` — 置換パラメーターの検証
  - search/replace が空の場合はエラー
  - isRegex=true で search が不正な正規表現の場合はエラー

---

### src/core/Models/SegmentItem.cs

#### `SegmentItem` record

UI セグメントコントロールの個別アイテム。

- **Content**: `string` — 表示テキスト
- **Value**: `object` — 対応する列挙値
- **IsEnabled**: `bool` — このアイテムが有効かどうか（デフォルト: true）

#### `SegmentDefine` record

UI セグメントコントロールの定義。

- **Label**: `string` — ラベルテキスト
- **Mode**: `ModePropDef` — バインド先の型安全なモード定義（ConvertConfig の Get/Set アクセサ）
- **Height**: `double` — コントロールの高さ（デフォルト: NaN = 自動）
- **Segments**: `SegmentItem[]?` — セグメントアイテム配列（null の場合はデフォルトセグメント使用）
- **ForceEnableState**: `bool?` — 強制的な有効/無効状態設定（null の場合は自動判定）

---

### src/core/Models/AppSetting.cs

#### `AppSetting` partial class

アプリケーションのウィンドウ設定を管理するモデルクラス。

**継承**: `SettingsPersistenceBase<AppSetting>`

**ObservableProperty**:

- `WindowWidth` (`double`) — ウィンドウの幅（デフォルト: 1000）
- `WindowHeight` (`double`) — ウィンドウの高さ（デフォルト: 800）

**コンストラクタ**: `AutoSaveFileName` を `%LOCALAPPDATA%\ClipboardZenHanConverter\AppSetting.json` に設定

**内部メソッド**:

- `ApplyFrom(AppSetting other)` — WindowWidth / WindowHeight をコピー

---

### src/core/Models/AppJsonContext.cs

#### `AppJsonContext` partial class

System.Text.Json ソースジェネレーター対応 JSON シリアライゼーションコンテキスト。

**シリアライズ対象**: `AppSetting`, `ConvertConfig`, `ReplacePair`

**オプション**: WriteIndented=true, DefaultIgnoreCondition=WhenWritingNull, PropertyNamingPolicy=Unspecified

---

### src/core/Models/SettingsPersistenceBase.cs

#### `SerializableTypeInfo` record

シリアライズに使用する JsonSerializerContext と型を保持するレコード。

- **Context**: `JsonSerializerContext`
- **Type**: `Type`

#### `SettingsPersistenceBase<T>` abstract class

設定の JSON ファイルへの自動永続化を提供する基底クラス。

**プロパティ**:

- `IsAutoSave` (`bool`) — 自動保存の有効/無効
- `AutoSaveFileName` (`string`) — 自動保存先のファイルパス

**メソッド**:

- `Initialize()` — 設定ファイルを読み込み、自動保存を開始
- `CancelPendingSave()` — 保留中の自動保存をキャンセル
- `SaveToJsonFile(string filePath)` → `void` — 同期的に JSON ファイルに保存
- `SaveToJsonFileAsync(string filePath, CancellationToken ct)` → `Task` — 非同期的に JSON ファイルに保存
- `LoadFromJsonFile(string filePath)` → `void` — JSON ファイルから設定を読み込み

**抽象メソッド**:

- `ApplyFrom(T other)` — 読み込んだ設定を現在のインスタンスに適用

**最適化**: 300ms のデバウンス付き自動保存（CancellationTokenSource によるキャンセル制御）

---

### src/core/Logic/CharConverter.cs

#### `CharConverter` partial class

ConvertConfig の設定に基づいて文字列の全角/半角変換を実行するコアロジック。

**実装**: `ITextConverter`, `IDisposable`

**プロパティ**:

- `Config` (`ConvertConfig`) — 変換設定

**内部レコード**:

- `MapEntry(Func<ConvertConfig, Enum> GetMode, object Entry, bool IsPair)` — モード取得デリゲートと EsUtil エントリのペア

**メソッド**:

- `Convert(string text)` → `string` — テキスト変換を実行
  - null/空文字はそのまま返す
  - IsEnabledZenHan=true の場合のみ全角/半角変換を実行
  - ZenHan 変換の有無に関わらずユーザー定義置換は常に適用
- `GetConvertPairs()` → `ConvertPairs` — 現在の Config に基づいて全変換ペアを生成（キャッシュ）
- `Dispose()` → `void` — リソース解放

**内部メソッド**:

- `ResolvePairs(Enum mode, object entry)` → `ConvertPairs` — モード種別とエントリから変換ペアを解決（静的）
- `GetYenConvertPairs(ZenHanEtcYenMode mode, string src)` → `ConvertPairs` — 円記号/バックスラッシュ変換ペア生成（静的）
- `ApplyUserReplacements(string text)` → `string` — ユーザー定義の置換ルール適用
- `SingleSpaceRegex()` → `Regex` — 連続スペース検出正規表現（GeneratedRegex）

**最適化**:

- `_cachedPairs` による変換ペアのキャッシュ（Config 変更時に自動無効化）
- `SymbolMap` 配列による宣言的なマッピング定義
- ソースジェネレーター（`[GeneratedRegex]`）による正規表現の事前コンパイル

---

### src/core/Helpers/ZenHanConverterExtension.cs

#### `ZenHanConverterExtension` static class

列挙型の変換モードに基づいて EsUtil の ConvertPairs を解決する拡張メソッド。

**拡張メソッド**:

- `GetConvertPairs(this ZenHanMode, IZenHanConverterToHanToZen)` → `ConvertPairs`
- `GetConvertPairs(this ZenHanKanaMode, IZenHanConverterToHanToZen)` → `ConvertPairs`
- `GetConvertPairs(this ZenHanEtcZenHanAsciiMode, object)` → `ConvertPairs`

---

### src/core/Interfaces/

#### `INavigationService` interface

ページ遷移の抽象化（DIP）。

- `NavigateTo(object? page)` → `void`
- `Initialize()` → `void`

#### `IClipboardService` interface

クリップボード操作の抽象化（DIP）。

- `GetTextAsync()` → `Task<string?>`
- `SetText(string text)` → `void`
- `Flush()` → `void`
- `ContentChanged` — `EventHandler<object>?`

#### `ITextConverter` interface

テキスト変換の抽象化（DIP）。

- `Convert(string text)` → `string`

---

## src/app プロジェクト

### src/app/App.xaml.cs

#### `App` partial class

アプリケーションのエントリポイント。

**静的プロパティ**:

- `Services` (`IServiceProvider`) — DI コンテナのサービスプロバイダー

**静的メソッド**:

- `GetService<T>()` → `T` — DI コンテナから指定型のサービスを取得

**メソッド**:

- `OnLaunched(LaunchActivatedEventArgs args)` — 起動時のメインウィンドウ表示と初期ページ設定

**処理フロー**:

1. コンストラクタで DI コンテナを初期化、AppSetting/ConvertConfig を自動保存モードで起動
2. OnLaunched でメインウィンドウを表示、NavigationService を初期化
3. 集約エラーハンドラー（UI/バックグラウンド/非同期タスク）を購読

---

### src/app/Services/NavigationService.cs

#### `NavigationService` class

アプリケーション内のページ遷移を管理するサービス。

**実装**: `INavigationService`

**内部構造**:

- `_pageMap` (`FrozenDictionary<string, Type>`) — ページ名と型のマッピング（"Home" → HomePage, "Settings" → SettingsPage）
- `_pageCache` (`Dictionary<Type, UIElement>`) — キャッシュされたページインスタンス
- `_currentPageType` (`Type?`) — 現在表示中のページタイプ

**メソッド**:

- `Initialize()` — SettingsPage を事前生成（DispatcherQueue.Low 優先度）
- `NavigateTo(object? selectedPage)` — NavigationViewItem または文字列でページ遷移
- `NavigateToPage(string tag)` — 指定タグ名のページへ遷移（キャッシュあり/なしの分岐、前ページの非表示化、NavigationView の選択状態同期）

**最適化**:

- `FrozenDictionary` による高速ルックアップ
- ページインスタンスのキャッシュによる再生成コスト削減

---

### src/app/Services/ClipboardService.cs

#### `ClipboardService` partial class

システムクリップボードの読み書きと内容変更監視を提供するサービス。

**実装**: `IClipboardService`

**プロパティ/イベント**:

- `ContentChanged` — クリップボード内容変更イベント

**メソッド**:

- `GetTextAsync()` → `Task<string?>` — クリップボードからテキストを非同期取得
- `SetText(string text)` → `void` — クリップボードにテキストを設定
- `Flush()` → `void` — クリップボード内容を永続化
- `RaiseContentChanged()` → `void` — テスト用にイベント発行

**内部メソッド**:

- `OnClipboardContentChanged(object? sender, object e)` — システムイベントハンドラ
- `ClipboardActionSafe(Action action)` — クリップボード操作を安全に実行（アクセス拒否時の例外を無視）

**セーフガード**: バックグラウンド時の `UnauthorizedAccessException` や WinRT 相互運用の `COMException` を握り潰し

---

### src/app/Services/DependencyInjectionExtensions.cs

#### `DependencyInjectionExtensions` static class

DI コンテナへのサービス登録拡張メソッド。

**登録サービス（全てシングルトン）**:

- サービス: `NavigationService`, `ClipboardService`
- コアロジック: `CharConverter`
- モデル: `AppSetting`, `ConvertConfig`
- ViewModel: `MainWindowViewModel`, `HomeViewModel`, `SettingsViewModel`
- View: `MainWindow`, `HomePage`, `SettingsPage`

---

### src/app/ViewModels/MainWindowViewModel.cs

#### `MainWindowViewModel` partial class

メインウィンドウのデータ管理とページ遷移制御。

**依存**: `INavigationService`

**ObservableProperty**:

- `SelectedPage` (`object?`) — 現在選択されているページ（変更時に NavigateTo を自動実行）

---

### src/app/ViewModels/HomeViewModel.cs

#### `HomeViewModel` partial class

ホーム画面のデータ管理、クリップボード監視と文字変換実行。

**実装**: `IDisposable`

**依存**: `ConvertConfig`, `ITextConverter`, `IClipboardService`, `INavigationService`, `SettingsViewModel?`（オプション）

**ObservableProperty**:

- `BeforeText` (`string`) — 変換前のテキスト
- `ConvertedText` (`string`) — 変換後のテキスト
- `SelectedPresetName` (`string?`) — 現在選択されているプリセット名（ホーム画面/設定画面で同期）

**プロパティ**:

- `Config` (`ConvertConfig`) — 変換設定
- `PresetNames` (`ObservableCollection<string>`) — プリセット名の一覧（SettingsViewModel と同期）
- `IsPresetSelected` (`bool`) — プリセット選択有無（ホーム画面 ComboBox の有効/無効制御に使用）
- `TestMode` (`bool`) — テスト用同期実行モード

**内部フィールド**:

- `_settingsViewModel` (`SettingsViewModel?`) — 同期用設定画面 ViewModel
- `_isUpdatingSelection` (`bool`) — 同期更新中フラグ（循環更新防止）

**コマンド**:

- `NavigateToSettings` (`[RelayCommand]`) — 設定画面へ遷移

**処理フロー**:

1. `ClipboardService.ContentChanged` イベント受信
2. `SemaphoreSlim` で排他制御
3. クリップボードからテキスト取得
4. `ITextConverter.Convert` で変換
5. 変換結果が元と異なる場合のみクリップボードに書き戻し
6. `_isUpdatingClipboard` フラグで書き戻し中の再帰イベントを防止

**同期ロジック**:

- `OnSelectedPresetNameChanged` — ユーザーがホーム画面のドロップダウンで選択 → SettingsViewModel.LoadPreset を呼び出し
- `SyncFromSettings()` — SettingsViewModel から PresetNames / SelectedPresetName を取得して同期
- `OnSettingsPropertyChanged` — SettingsViewModel の PropertyChanged を監視し、変更を自動検出して同期

---

### src/app/ViewModels/SettingsViewModel.cs

#### `SettingsViewModel` partial class

設定画面のデータ管理。

**依存**: `ConvertConfig`

**ObservableProperty**:

- `SelectedPresetName` (`string?`) — 現在選択されているプリセット名

**プロパティ**:

- `ConvertConfig` (`ConvertConfig`) — 変換設定への参照
- `NumberItems` / `AlphabetItems` / `KanaItems` / `SymbolItems` / `EtcZenHanAsciiItems` / `EtcBslashYenItems` / `EtcSpecialItems` / `EtcMultiSpaceItems` — 各変換カテゴリのセグメントアイテム一覧
- `PresetNames` (`ObservableCollection<string>`) — プリセット名の一覧
- `ReplaceItems` (`ObservableCollection<ReplacePairItem>`) — ユーザー定義の置換ルール一覧

**メソッド**:

- `ValidateAllAndSyncToConfig()` — 全置換行をバリデーションし有効な行のみを同期
- `ValidateItemAndSync(ReplacePairItem item)` — 指定行をバリデーションし全行同期
- `ReloadReplaceItemsFromConfig()` — ReplaceItems を ConvertConfig から再構築
- `ExportSettings(string filePath)` — JSON エクスポート
- `ImportSettings(string filePath)` → `string?` — JSON インポート
- `RefreshPresets()` — プリセット一覧を更新
- `SavePreset(string name)` — 現在の設定をプリセットとして保存
- `LoadPreset(string? name)` — プリセットを読み込み（選択名を維持、RefreshSelectedPreset による上書きは行わない）
- `DeletePreset(string? name)` — プリセットを削除

**イベント駆動**:

- `ConvertConfig.PropertyChanged` 購読 → 設定変更を検出し `RefreshSelectedPreset()` を呼び出し
- `OnSelectedPresetNameChanged` — ドロップダウン選択によるプリセット読み込み
- `_isUpdatingSelection` フラグで再帰的読み込みを防止

---

### src/app/ViewModels/ZenHanConvertItem.cs

#### `ZenHanConvertItem` partial class

設定画面の変換項目を表す ViewModel。

**実装**: `IDisposable`

**プロパティ**:

- `Label` (`string`) — 項目のラベル
- `IsEnabled` (`bool`) — コントロールの有効状態
- `ForceEnableState` (`bool?`) — 強制的な有効状態設定
- `Segments` (`SegmentItem[]`) — 選択可能なセグメント定義
- `SelectedLabel` (`string`) — 選択されている項目のラベル（getter/setter）

**最適化**:

- `ConvertConfig.Mode`（型安全なアクセサ定義）を介したプロパティアクセス
- `PropertyChanged` 購読で `SelectedLabel` を再通知

---

### src/app/ViewModels/ReplacePairItem.cs

#### `ReplacePairItem` partial class

置換ルール1行分の ViewModel。

**ObservableProperty**:

- `Search` (`string`) — 検索文字列
- `Replace` (`string`) — 置換文字列
- `IsRegex` (`bool`) — 正規表現フラグ
- `ErrorMessage` (`string?`) — バリデーションエラーメッセージ

**メソッド**:

- `Validate()` → `string?` — バリデーション実行（ReplacePair.TryValidate に委譲）
- `ToPair()` → `ReplacePair` — ViewModel からモデルへの変換

---

### src/app/Views/MainWindow.xaml / MainWindow.xaml.cs

#### `MainWindow` sealed partial class

アプリケーションのメインウィンドウ。

**構造**: TitleBar + NavigationView（Home/Settings） + ContentFrame（ページコンテンツ領域）

**プロパティ**:

- `ViewModel` (`MainWindowViewModel`)
- `ContentFrame` (`Grid`) — ページコンテンツを表示
- `NavigationView` (`NavigationView`)
- `TitleBar` (`TitleBar`)

**処理**: アクティブ化時にウィンドウサイズを AppSetting から復元、クローズ時に保存

---

### src/app/Views/HomePage.xaml / HomePage.xaml.cs

#### `HomePage` sealed partial class

ホーム画面を表示するページ。

**プロパティ**: `ViewModel` (`HomeViewModel`)

**UI**: 変換前/変換後のテキスト表示 + プリセット選択 ComboBox（歯車アイコンの左側） + 設定画面へのナビゲーションボタン（歯車アイコン）

---

### src/app/Views/SettingsPage.xaml / SettingsPage.xaml.cs

#### `SettingsPage` sealed partial class

設定画面を表示するページ。

**プロパティ**: `ViewModel` (`SettingsViewModel`)

**UI レイアウト（Grid.Row 3段構成）**:

1. **Grid.Row="0"（固定）**: ヘッダー（全角/半角の変換設定 ToggleSwitch）
2. **Grid.Row="1"（固定）**: **設定の管理**（Border 使用、ヘッダー直下に固定）
   - 左寄せ: プリセット選択 ComboBox ＋「プリセット編集」ボタン
   - 右寄せ: 「エクスポート (JSON)」＋「インポート (JSON)」ボタン
3. **Grid.Row="2"（スクロール）**: ScrollViewer 内の設定項目
   - 8つの変換カテゴリ（数字/英字/かな/記号/その他）
   - 文字列の置換（DataGrid）

**内部メソッド** (`internal static`):

- `ContainsInvalidFileNameChars(string name)` → `bool` — ファイル名に使用できない文字が含まれるかの判定
- `ContainsBuiltInKeyword(string name)` → `bool` — built-in/builtin キーワードを含むかの判定（NFKC 正規化 + 小文字変換後）

**イベントハンドラ**:

- `OnEditPresetClick` — プリセット編集ダイアログ表示（編集可能 ComboBox + 保存/削除/閉じる）
- `OnPresetComboBoxSelectionChanged` — プリセット選択ドロップダウンの変更処理
- `OnEditReplaceRowClick` — 置換行編集ダイアログ表示
- `OnExportSettingsClick` / `OnImportSettingsClick` — 設定のエクスポート/インポート（FilePicker）

---

### src/app/Helpers/SegmentDefinitions.cs

#### `SegmentDefinitions` static class

設定画面のセグメントコントロール定義を提供。

**定義配列**:

- `NumberDefs` (`SegmentDefine[]`) — 数字変換モード
- `AlphabetDefs` (`SegmentDefine[]`) — 英字変換モード
- `KanaDefs` (`SegmentDefine[]`) — かな変換モード（半角カナ/全角カタカナ/全角ひらがなの3定義）
- `SymbolDefs` (`SegmentDefine[]`) — 記号変換モード（30定義）
- `EtcZenHanAsciiDefs` (`SegmentDefine[]`) — かな約物変換モード
- `EtcBslashYenDefs` (`SegmentDefine[]`) — バックスラッシュ/円記号変換モード
- `EtcSpecialDefs` (`SegmentDefine[]`) — 特殊文字（タブ/改行）変換モード
- `EtcMultiSpaceDefs` (`SegmentDefine[]`) — 連続スペース変換モード

---

### src/app/Helpers/EnableStyleSelector.cs

#### `EnableStyleSelector` class

SegmentedItem の有効/無効状態に応じたスタイルセレクター。

---

### src/app/Converters/

#### `BoolToCheckMarkConverter` sealed partial class

bool 値をチェックマーク文字列に変換（IValueConverter）。

- `Convert`: true → "✅", false/null → "□"
- `ConvertBack`: 未サポート（NotSupportedException）

#### `StringNotEmptyToVisibilityConverter` sealed partial class

空文字列でない場合に Visible にするコンバーター（IValueConverter）。

- `Convert`: null/空文字 → Collapsed, それ以外 → Visible
- `ConvertBack`: 未サポート（NotSupportedException）

---

## テストプロジェクト

### フォルダ構成

```text
test/
├── TestHelper.cs                    # テスト用共通ヘルパー
├── core/
│   ├── Logic/
│   │   └── CharConverterTests.cs    # 変換ロジックのテスト
│   └── Models/
│       ├── AppSettingTests.cs       # AppSetting のテスト
│       ├── ConvertConfigTests.cs    # ConvertConfig のテスト
│       ├── ReplacePairTests.cs      # ReplacePair のテスト
│       └── SegmentItemTests.cs      # SegmentItem のテスト
└── app/
    ├── Converters/
    │   ├── BoolToCheckMarkConverterTests.cs
    │   └── StringNotEmptyToVisibilityConverterTests.cs
    ├── Helpers/
    │   └── SegmentDefinitionsTests.cs
    ├── ViewModels/
    │   ├── HomeViewModelTests.cs
    │   ├── MainWindowViewModelTests.cs
    │   ├── ReplacePairItemTests.cs
    │   └── SettingsViewModelTests.cs
    └── Views/
        └── SettingsPagePresetValidationTests.cs
```

---

## 動作仕様

### クリップボード監視

1. アプリ起動後、システムクリップボードの変更イベントを購読
2. 変更を検出すると `SemaphoreSlim` で排他制御
3. クリップボードからテキストを取得し、設定に基づいて変換
4. 変換結果が元のテキストと異なる場合のみクリップボードに書き戻し
5. 書き戻し中の再帰イベントは `_isUpdatingClipboard` フラグで防止

### ページ遷移

1. `NavigationView.SelectedItem` の変更 → MainWindowViewModel.SelectedPage → NavigationService.NavigateTo
2. プログラムからの遷移（ホーム画面の歯車ボタン）→ NavigationView.SelectedItem を同期更新
3. ページインスタンスは DI コンテナ経由で生成され、キャッシュされる
4. 前ページは Visibility.Collapsed で非表示化

### 設定の自動保存

1. `ConvertConfig` または `AppSetting` のプロパティ変更を検出
2. 300ms のデバウンス後、JSON ファイルに非同期保存
3. デバウンス中にさらに変更があった場合はタイマーがリセット

### プリセットのマッチング

1. `GetPresetNames()` と同じ順序（組込み→保存、各グループ内で名前昇順）で先頭から一致確認
2. 最初に見つかった一致するプリセット名を `SelectedPresetName` に設定
3. 設定内容の変更は `ConvertConfig.PropertyChanged` イベントで検出
4. `LoadPreset` 実行時は `_isUpdatingSelection` フラグで再帰を防止

### プリセット編集ダイアログのバリデーション

| 条件 | メッセージ | 保存ボタン | 削除ボタン |
| --- | --- | --- | --- |
| 空文字 | なし | 無効 | 無効 |
| 組込みプリセット | 組込みプリセットです。 | 無効 | 無効 |
| ファイル名に使用不可文字を含む | 使用できない文字が含まれます。 | 無効 | 既存なら有効 |
| 半角小文字で built-in/builtin を含む | built-in または builtin は使用できません。 | 無効 | 既存なら有効 |
| 既存ユーザープリセット | 既に存在します。 | 有効 | 有効 |
| 新規有効な名前 | なし | 有効 | 無効 |

バリデーションは選択変更時、フォーカス喪失時、およびテキスト内容変化時にリアルタイム実行。

---

## パフォーマンス向上施策

- **変換ペアのキャッシュ**: `CharConverter._cachedPairs` により Config 変更時のみ再計算
- **メタデータ駆動の DRY 設計**: `ConvertConfig.Mode.All` 配列からコピー/比較/全角設定のアクション配列を動生成
- **FrozenDictionary**: ページマップと組み込みプリセットの高速ルックアップ
- **デバウンス保存**: 300ms のデバウンスで不要なファイル書き込みを抑制
- **型安全なモードアクセサ**: `ConvertConfig.Mode`（`ModePropDef`）でプロパティ名文字列・リフレクションを排除
- **GeneratedRegex**: 連続スペース検出の正規表現をコンパイル時生成
- **SettingsPage の事前生成**: NavigationService.Initialize で DispatcherQueue.Low 優先度で非同期にページインスタンスを事前生成
