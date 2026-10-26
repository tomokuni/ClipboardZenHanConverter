# ZenHanConverter.Core

クリップボードの全角/半角自動変換アプリのコアロジック・モデル・プラットフォーム API ライブラリです。
**MewUI にも WinUI3 にも依存しない**ため、両方の UI 実装から共有できます。
`src/app_MewUI`（MewUI アプリ）から参照されます。

## 構成

- `Enums/` — 変換モード列挙型（`ZenHanMode` ほか）
- `Models/` — `ConvertConfig`, `ReplacePair`, `SegmentItem`, `SegmentDefinitions`, `AppSetting`, `SettingsPersistenceBase`
- `Interfaces/` — `IClipboardService`, `ITextConverter`
- `Logic/` — `CharConverter`（変換ロジック）, `ClipboardChangeDetector`（クリップボード変更検出）
- `Helpers/` — `ZenHanConverterExtension`（変換ペア解決拡張）, `WindowPlacement`（ウィンドウ位置補正）, `SplitLayout`（上下 2 分割の比率）
- `Native/` — 仮想画面と DIP ⇔ 物理ピクセルの換算（`ScreenVisibleArea`）
- `Icons/` — Fluent Icons の SVG アセットとパスデータ提供（`FluentIconData`）

## 依存

- `CommunityToolkit.Mvvm` — ObservableProperty 生成・変更通知
- `EsUtil.Feature.ZenHan.Core`（0.1.* のパッケージ参照）— 全角/半角変換ペア
- `EsUtil.Platform.Core`（0.2.* のパッケージ参照）— 幾何型（`EsPoint` / `EsSize` / `EsRect`）・DPI 換算・クリップボードと仮想画面の Win32 実装

UI フレームワークには依存しません。 Win32 API の呼び出しは `EsUtil.Platform.Core` の `Native.Win` が単一所有します。

## 主なクラス

- `ConvertConfig` — 変換設定（自動永続化・プリセット管理）
- `SegmentDefinitions` — 設定画面の変換カテゴリ定義（ラベル・セグメント項目・バインド先）
- `CharConverter` — テキスト変換の実行
- `ClipboardChangeDetector` — シーケンス番号によるクリップボード内容変更の検出
- `WindowPlacement` — 保存位置の表示領域内への補正（DIP の幾何型は `EsUtil.Platform.Core`）
- `ScreenVisibleArea` — 仮想画面の取得（`EsUtil.Platform.Core` の `WinVirtualScreen`）と DIP ⇔ 物理ピクセルの換算
- `FluentIconData` — Fluent Icons の SVG パスデータ（MewUI / WinUI3 の双方で利用可能）