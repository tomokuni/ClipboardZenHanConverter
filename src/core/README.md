# ClipboardZenHanConverter.Core

クリップボードの全角/半角自動変換アプリのコアロジック・モデル・プラットフォーム API ライブラリです。
**MewUI にも WinUI3 にも依存しない**ため、両方の UI 実装から共有できます。
`src/app_MewUI`（MewUI アプリ）から参照されます。

## 構成

- `Enums/` — 変換モード列挙型（`ZenHanMode` ほか）
- `Models/` — `ConvertConfig`, `ReplacePair`, `SegmentItem`, `SegmentDefinitions`, `AppSetting`, `SettingsPersistenceBase`
- `Interfaces/` — `IClipboardService`, `ITextConverter`
- `Logic/` — `CharConverter`（変換ロジック）, `ClipboardChangeDetector`（クリップボード変更検出）
- `Helpers/` — `ZenHanConverterExtension`（変換ペア解決拡張）, `WindowPlacement`（ウィンドウ位置補正）
- `Geometry/` — DIP 座標の幾何型（`PointD`, `SizeD`, `RectD`）
- `Native/` — Win32 API ラッパー（`Win32Clipboard`, `Win32Display`, `PixelBounds`）
- `Icons/` — Fluent Icons の SVG アセットとパスデータ提供（`FluentIconData`）

## 依存

- `CommunityToolkit.Mvvm` — ObservableProperty 生成・変更通知
- `EsUtil.Helper.ZenHanConverter` — 全角/半角変換ペア

UI フレームワークおよび Win32 以外の OS API には依存しません。

## 主なクラス

- `ConvertConfig` — 変換設定（自動永続化・プリセット管理）
- `SegmentDefinitions` — 設定画面の変換カテゴリ定義（ラベル・セグメント項目・バインド先）
- `CharConverter` — テキスト変換の実行
- `ClipboardChangeDetector` — シーケンス番号によるクリップボード内容変更の検出
- `WindowPlacement` — 保存位置の表示領域内への補正
- `Win32Clipboard` — クリップボードの読み書きとシーケンス番号取得
- `Win32Display` — 仮想画面（全モニタの外接矩形）の取得
- `FluentIconData` — Fluent Icons の SVG パスデータ（MewUI / WinUI3 の双方で利用可能）
