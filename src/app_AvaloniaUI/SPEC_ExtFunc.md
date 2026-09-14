# ClipboardZenHanConverter.App.AvaloniaUI 外部機能仕様書

本ドキュメントは `src/app_AvaloniaUI`（Avalonia UI アプリ）の公開 API 仕様（外部機能仕様）を記載します。
メソッド内部のコードは記載しません。外部利用者に関係する情報のみを扱います。

## 共有コア

モデル・変換ロジック・Win32 API・アイコンデータ・変換カテゴリ定義は `src/core`（UI 非依存）が提供します。
公開 API は `../core/SPEC_ExtFunc.md` を参照してください。

## 公開型一覧

### ViewModels

設定画面の ViewModel は **共有プレゼンテーション層（`src/core_presentation`）** が提供します。
公開 API は `../core_presentation/SPEC_ExtFunc.md` を参照してください。

本アプリ固有の ViewModel は次のとおりです。

- `MainWindowViewModel` — ナビゲーションと画面解決、タイトルバーが参照する設定の公開
- `HomeViewModel` — ホーム画面（クリップボード監視と文字変換）
- `NavigationItem` — ナビゲーションペインの項目（アイコン形状が Avalonia の `Geometry` のため本アプリに残す）

#### `MainWindowViewModel : ObservableObject`

メインウィンドウのデータ管理（ナビゲーションと画面解決、タイトルバーが参照する設定の公開）。

- `AppSetting AppSetting` — アプリ設定（タイトルバーのクリップボード変換スイッチが参照）
- `SettingsViewModel Settings` — 設定画面の ViewModel（タイトルバーのプリセット選択が参照）
- `IReadOnlyList<NavigationItem> NavItems` — ナビゲーションペインの項目一覧
- `NavigationItem? SelectedNavItem` — 選択中のナビゲーション項目
- `Control? CurrentView` — 現在表示中のビュー（選択項目の変更で解決される）

#### `NavigationItem`

ナビゲーションペインの項目。

- `string Tag` — ページタグ（"Home" / "Settings"）
- `string Label` — 表示ラベル
- `Geometry Icon` — 項目アイコンの形状

#### `HomeViewModel : IDisposable`

ホーム画面のデータ管理、クリップボード監視と文字変換実行。

- `string BeforeText` — 変換前テキスト（クリップボード取り込みまたは手動入力。変更時に自動変換）
- `string ConvertedText` — 変換後テキスト
- `bool TestMode` — テスト用の同期実行モード

全角/半角変換は常に有効（オン/オフの切り替えはありません）。クリップボードの読み書き可否は `AppSetting.IsClipboardConvertEnabled` に従います。  
クリップボード変換スイッチとプリセット選択はタイトルバー（`MainWindow`）が単一所有し、この ViewModel は関与しません。

### Views（`EsUtil.ClipboardZenHanConverter.App.AvaloniaUI.Views`）

#### `MainWindow : Window`

メインウィンドウ。カスタムタイトルバー（アプリ名・クリップボード変換スイッチ・プリセット選択・ウィンドウ操作ボタン）、ナビゲーションペイン、ウィンドウ位置・サイズの復元/保存。

- 装飾は `WindowDecorations="None"` としてアプリが描画し、タイトルバー領域に `WindowDecorationProperties.ElementRole="TitleBar"` を指定してドラッグ移動とダブルクリック最大化を有効にします。
- タイトルバー内の操作系コントロールには `ElementRole="User"` を指定し、ドラッグ領域による入力の横取りから除外します。
- ウィンドウ外周（各辺・各角）に `Resize*` の役割を宣言し、リサイズを可能にします。
- 起動時は `AppSetting.WindowWidth` / `WindowHeight`（DIP）をサイズとして復元し、`WindowX` / `WindowY`（DIP）を物理ピクセルへ換算して適用します（表示領域外は Core の `ScreenVisibleArea` で補正）。
- 終了時は現在のサイズと位置を DIP へ換算して `AppSetting` へ保存し、同期で JSON 保存します。
- 最大化/最小化中に終了した場合は、通常状態での最後の位置とサイズを保存します。

#### `HomeView : UserControl`

ホーム画面。変換前（編集可能）/変換後のテキスト表示。

#### `SettingsView : UserControl`

設定画面。カテゴリ別の変換設定（セグメント選択）、文字列の置換、プリセット選択・編集、JSON のエクスポート/インポート。

- ファイルの選択は Avalonia の StorageProvider（`TopLevel.StorageProvider`）で行い、パス解決のみを View が担います。

#### `PresetEditDialog : Window`

プリセットの保存・削除ダイアログ。名前の検証結果に応じて保存/削除ボタンが有効/無効になります。

#### `MessageDialog : Window`

メッセージ表示用のシンプルなダイアログ。

- `static Task ShowAsync(Control owner, string message)` — 指定コントロールを所有ウィンドウとしてメッセージを表示

#### `CategorySection : UserControl`（`Views.Controls`）

変換カテゴリ 1 セクション（見出し・説明・変換項目一覧・補足）を表示する再利用コントロール。

- `string Header` / `string Description` / `string Remark` — 見出し・説明・補足（空の場合は非表示）
- `IEnumerable? Items` — 表示する変換項目（`ZenHanConvertItem` の列挙）

### Services（`EsUtil.ClipboardZenHanConverter.App.AvaloniaUI.Services`）

#### `INavigationService` interface

タグからビューを解決する抽象（DIP）。

- `Control ResolveView(string tag)` — タグに対応するビュー（同一タグは同一インスタンス）
- `void PreloadView(string tag)` — UI スレッドのアイドル時にビューを事前生成

#### `NavigationService : INavigationService`

タグとビュー型のマッピング（`FrozenDictionary`）とビューのキャッシュを保持します。

#### `ClipboardService : IClipboardService`

クリップボードの読み書きと変更監視。読み書きは Core の `Win32Clipboard`、変更検出は Core の `ClipboardChangeDetector` へ委譲し、`DispatcherTimer` でポーリングします。

- `event EventHandler<object>? ContentChanged` — 内容変更イベント（UI スレッドで発行）
- `Task<string?> GetTextAsync()` / `void SetText(string text)` / `void Flush()`
- `void RaiseContentChanged()` — テスト用の発火

### Helpers（`EsUtil.ClipboardZenHanConverter.App.AvaloniaUI.Helpers`）

#### `FluentIcons` static class

Core のパスデータから Avalonia のアイコン形状を提供します。パスデータは Core の `FluentIconData` が単一所有し、初回アクセス時に一度だけ生成します。

- `Geometry ConvertRange` — ホーム項目用の「Convert Range」アイコン形状
- `Geometry Settings` — 設定項目用の「Settings」アイコン形状

### Converters（`EsUtil.ClipboardZenHanConverter.App.AvaloniaUI.Converters`）

#### `StringNotEmptyToBoolConverter`

文字列が空でない場合に `true` を返します（任意項目の `IsVisible` へのバインドに使用）。

- `static readonly StringNotEmptyToBoolConverter Instance` — 共有インスタンス
