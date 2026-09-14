# ClipboardZenHanConverter.App.MewUI 外部機能仕様書

本ドキュメントは `src/app_MewUI`（MewUI アプリ）の公開 API 仕様（外部機能仕様）を記載します。
メソッド内部のコードは記載しません。外部利用者に関係する情報のみを扱います。

## 公開型一覧

### ViewModels

設定画面の ViewModel は **共有プレゼンテーション層（`src/core_presentation`）** が提供します。
公開 API は `../core_presentation/SPEC_ExtFunc.md` を参照してください。

本アプリ固有の ViewModel は `HomeViewModel`（ホーム画面）です。

#### `HomeViewModel : IDisposable`

ホーム画面のデータ管理、クリップボード監視と文字変換実行。

- `string BeforeText` — 変換前テキスト（クリップボード取り込みまたは手動入力。変更時に自動変換）
- `string ConvertedText` — 変換後テキスト
- 全角/半角変換は常に有効（オン/オフの切り替えはありません）。クリップボード変換（コピー検知・書き戻し）の有効/無効は `AppSetting.IsClipboardConvertEnabled` に従います。

### Views（`ClipboardZenHanConverter.App.MewUI.Views`）

#### `MainWindow : Window`

メインウィンドウ。カスタムタイトルバー（アプリ名・クリップボード変換スイッチ・プリセット選択・ウィンドウ操作ボタン）、ウィンドウサイズの復元/保存、NavigationView によるペイン式ナビゲーションとコンテンツ表示。

- カスタムタイトルバーには「clipboard text converter」、クリップボード変換の有効/無効スイッチ、プリセット選択ドロップダウン、ウィンドウ操作ボタンを表示します。
- ウィンドウ操作ボタンは最小化・最大化（最大化中は復元）・閉じるの 3 つで、それぞれ `Window` のウィンドウ操作を呼び出します。
- 起動時は `AppSetting.WindowWidth` / `WindowHeight`（サイズ）と `WindowX` / `WindowY`（位置）を復元し、終了時は現在の値（最大化/最小化中は通常状態での最後の値）を `AppSetting` へ保存します。
- 保存位置が表示領域外の場合は、一部が外なら表示領域内へ詰め、完全に外なら原点へ移動して表示します。
- クリップボード変換スイッチはタイトルバーが単一所有し、`AppSetting.IsClipboardConvertEnabled` に双方向で連動します。
- プリセット選択ドロップダウンは設定画面と同一仕様で、`SettingsViewModel.SelectedPresetName` に連動します（クリップボード変換スイッチの有効/無効では Disable になりません）。
- タイトルバーのアプリ名・余白領域のマウスダウンでウィンドウをドラッグ移動できます。同領域のダブルクリックは最大化ボタンと同じ動作（通常時は最大化、最大化中は復元）になります。
- NavigationView のペイン項目（ホーム / 設定）はアイコン付きで表示します。アイコン形状は `FluentIcons` が提供します。
- NavigationView のペイン配置は `Inline` 固定です（MewUI 既定の `Auto` は利用可能幅 1000 DIP 未満でペインを内容へ重ねる形へ切り替わり、狭めるとペインが見えなくなるため）。
- プロパティは公開しておらず、NavigationView がナビゲーション状態とコンテンツ領域を所有します。

### Helpers（`ClipboardZenHanConverter.App.MewUI.Helpers`）

#### `FluentIcons`

アプリで使用する Fluent Icons のアイコン形状を提供します。パスデータは Core の `FluentIconData`（SVG の埋め込みリソース）が単一所有し、MewUI の形状への変換のみを担います。初回アクセス時に一度だけ生成します。

- `PathGeometry ConvertRange` — ホーム項目用の「Convert Range」アイコン形状
- `PathGeometry Settings` — 設定項目用の「Settings」アイコン形状

#### `HomeView : UserControl`

ホーム画面。変換前/後表示、変換前テキストへの手動入力。プリセット選択はタイトルバーが提供します。

全角/半角変換は常に有効で、ホーム画面に有効/無効の切り替えはありません。

#### `SettingsView : UserControl`

設定画面。変換カテゴリ（セグメント）、文字列の置換、プリセット管理、エクスポート/インポート。
項目数が多い「記号の変換」はマルチカラム（`MultiColumnPanel`）で表示します。
変換項目行のラベルは「名称　記号」形式を名称部と記号部に分けて表示し、記号部とセグメントコントロールが縦に揃うように配置します。
セグメントの表示は設定値の変更を購読するため、プリセット読み込み・インポートで設定が変わると表示も追従します。

#### `MultiColumnPanel : Panel`

子要素を縦方向の複数列へ配置するパネル。列ごとに子要素を積み、最大列高さを最小化します。
列幅はエンジンが列ごとに算出します（列幅 = その列の最大アイテム幅）。子要素の幅が揃っていれば各列の幅も揃います。

- `double RowSpace` — 行間スペース（DIP、既定 4）
- `double ColumnSpace` — 列間スペース（DIP、既定 32）
- `int ColumnLimit` — 使用する列数の上限（既定 1）
- `const double DefaultRowSpace` / `const double DefaultColumnSpace` / `const int DefaultColumnLimit` — 各プロパティの既定値
- 実際の列数は幅に応じて上限以下へ減ります。幅が確定できない場合は単列として扱います。

### Services（`ClipboardZenHanConverter.App.MewUI.Services`）

#### `ClipboardService : IClipboardService`

クリップボードの読み書きと変更監視。Win32 の `GetClipboardSequenceNumber` ポーリングで変更を検出します。
Win32 API の呼び出しは `ClipboardZenHanConverter.Core.Native.Win32Clipboard`、変更検出は `ClipboardZenHanConverter.Core.Logic.ClipboardChangeDetector` へ委譲します。
ポーリングは `DispatcherTimer` で行い、`ContentChanged` を UI スレッドで発行します（購読側がバインド済みプロパティを更新するため）。

- `event EventHandler<object>? ContentChanged`

#### `INavigationService`

ナビゲーションの抽象。NavigationView のコンテンツ解決（タグからビュー生成・キャッシュ）を担います。

- `Element ResolveView(string tag)` — タグに対応するビューを生成（キャッシュ）して返す

#### `NavigationService : INavigationService`

NavigationView.ContentSelector からのビュー解決を実装し、HomeView / SettingsView をキャッシュします。

- `Element ResolveView(string tag)` — タグからビューを解決

## エントリポイント

`Program.Main` で DI コンテナを構築し、MewUI の `ApplicationBuilder` でメインウィンドウを表示します。
