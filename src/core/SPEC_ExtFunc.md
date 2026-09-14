# ClipboardZenHanConverter.Core 外部機能仕様書

本ドキュメントは `src/core` プロジェクトの公開 API 仕様（外部機能仕様）を記載します。
メソッド内部のコードは記載しません。外部利用者に関係する情報のみを扱います。

## 公開型一覧

### 列挙型（`ClipboardZenHanConverter.Core.Enums`）

#### `ZenHanMode`

全角/半角変換の基本方向。

| 値 | 名称 | 説明 |
| --- | --- | --- |
| 0 | None | 変換なし |
| 1 | ToHan | 全角文字を半角に変換 |
| 2 | ToZen | 半角文字を全角に変換 |

#### `ZenHanKanaMode`

かな文字（半角カナ/全角カタカナ/全角ひらがな）の変換方向。

| 値 | 名称 | 説明 |
| --- | --- | --- |
| 0 | None | 変換なし |
| 1 | ToHan | 全角かなを半角カナへ |
| 2 | ToZenKata | 半角カナ/全角ひらがなを全角カタカナへ |
| 3 | ToZenHira | 半角カナ/全角カタカナを全角ひらがなへ |

#### `ZenHanEtcZenHanAsciiMode`

かな約物（長音/読点/句点）の変換モード。

| 値 | 名称 | 説明 |
| --- | --- | --- |
| 0 | None | 変換なし |
| 1 | ToHan | 全角を半角へ |
| 2 | ToZen | 半角を全角へ |
| 3 | ToAscii | 対応する ASCII 文字へ（例: ー → -） |

#### `ZenHanEtcYenMode`

円記号/バックスラッシュの変換モード。

| 値 | 名称 | 説明 |
| --- | --- | --- |
| 0 | None | 変換なし |
| 1 | ToHanBSlash | 半角バックスラッシュ（\）へ |
| 2 | ToZenBSlash | 全角バックスラッシュ（＼）へ |
| 3 | ToHanYen | 半角円記号（¥）へ |
| 4 | ToZenYen | 全角円記号（￥）へ |

#### `ZenHanEtcSpecial`

特殊文字（タブ/改行/連続スペース）の変換モード。

| 値 | 名称 | 説明 |
| --- | --- | --- |
| 0 | None | 変換なし |
| 1 | Remove | 除去 |
| 2 | ToHanSpace | 半角スペース |
| 3 | ToZenSpace | 全角スペース |

### データモデル（`ClipboardZenHanConverter.Core.Models`）

#### `ConvertConfig`

全角/半角変換の全設定を管理します。`SettingsPersistenceBase<ConvertConfig>` を継承し、JSON への自動保存に対応します。

主要な公開メンバー:

- `bool IsEnabledZenHan` — 全角/半角変換の有効/無効（既定 `false`）。MewUI 版・WinUI 3 版とも参照せず、全角/半角変換は常に有効です（設定ファイル上の値として保持し、プリセット適用時に `true` になります）
- 各種 `ConvertMode*` プロパティ — 文字カテゴリごとの変換モード
- `List<ReplacePair> ReplacePairs` — ユーザー定義の置換ルール一覧
- `string[] GetPresetNames()` — 利用可能なプリセット名一覧
- `void SavePreset(string name)` / `bool LoadPreset(string name)` / `void DeletePreset(string name)`
- `static string PresetDirectory` — ユーザープリセットの保存ディレクトリ（既定 `%LOCALAPPDATA%\ClipboardZenHanConverter\Presets`、既定値は設定で切替可能）
- `bool IsBuiltInPreset(string name)`
- `string? FindMatchingPreset()` — 現在の設定に一致するプリセット名を返す
- `void ExportToFile(string filePath)` / `bool ImportFromFile(string filePath)`
- 組み込みプリセット名の定数 `BuiltInPresetAccountingPower` / `BuiltInPresetAlphanumericHanKanaZen`
- `static class Mode` — 全モードプロパティの型安全なアクセサ定義（`ModePropDef`）を公開。UI は文字列名ではなく `ModePropDef` を直接参照する（NativeAOT 対応）

#### 組み込みプリセットの内容

| プリセット名 | 数値・英字 | 記号 | かな | その他 |
| --- | --- | --- | --- | --- |
| 全力会計（Built-in） | 半角 | `()` `-` `/` `,` `.` 空白は半角、それ以外の記号は全角 | 半角カナ→全角カタカナ。全角カタカナ/ひらがなは変換なし | バックスラッシュ/円記号→半角円記号、タブ/改行/連続スペース→半角スペース |
| 英数記号半角、かな全角（Built-in） | 半角 | 全て半角 | 半角カナ→全角カタカナ。全角カタカナ/ひらがなは変換なし | かな約物は全角、改行は変換なし、タブ/連続スペース→半角スペース |

いずれも適用時に `ReplacePairs` を空にし、`IsEnabledZenHan` を `true` にします。

#### `ModePropDef`

`ConvertConfig` のモードプロパティへ型安全にアクセスする `record`。

| プロパティ | 型 | 説明 |
| --- | --- | --- |
| Get | `Func<ConvertConfig, object>` | 現在値を取得するデリゲート |
| Set | `Action<ConvertConfig, object>` | 値を設定するデリゲート |
| ToZenValue | `object?` | 「全力会計」プリセットで全角にする値（null は全角設定の対象外） |

#### `ReplacePair`

ユーザー定義の文字列置換ペア（`record`）。

| プロパティ | 型 | 説明 |
| --- | --- | --- |
| Search | string | 置換元（正規表現可） |
| Replace | string | 置換先 |
| IsRegex | bool | 正規表現として扱うか |

- `static bool TryValidate(string search, string replace, bool isRegex, out string? errorMessage)` — 検証

#### `SegmentItem` / `SegmentDefine`

UI セグメントコントロールの定義レコード。

- `SegmentItem(string Content, object Value, bool IsEnabled = true)` — 個別セグメント（表示テキスト / 値 / 有効状態）
- `SegmentDefine(string Label, ModePropDef Mode, double Height = NaN, SegmentItem[]? Segments = null, bool? ForceEnableState = null)` — 1 変換項目の定義。`Mode` でバインド先モードを型安全に参照

#### `SegmentDefinitions`

設定画面の変換カテゴリ定義を提供する静的クラス。UI フレームワークに依存しないため Core に配置しています。

| メンバー | 型 | 説明 |
| --- | --- | --- |
| `NumberDefs` / `AlphabetDefs` | `SegmentDefine[]` | 数字・英字の変換項目 |
| `KanaDefs` | `SegmentDefine[]` | かな（半角カナ / 全角カタカナ / 全角ひらがな）の変換項目 |
| `SymbolDefs` | `SegmentDefine[]` | 記号（括弧 / 引用符 / 演算子）とかな記号の変換項目 |
| `EtcZenHanAsciiDefs` | `SegmentDefine[]` | かな約物（長音 / 読点 / 句点）の変換項目 |
| `EtcBslashYenDefs` | `SegmentDefine[]` | バックスラッシュ / 円記号の変換項目 |
| `EtcSpecialDefs` | `SegmentDefine[]` | 特殊文字（タブ / 改行）の変換項目 |
| `EtcMultiSpaceDefs` | `SegmentDefine[]` | 連続スペースの変換項目 |

各項目はバインド先を `ConvertConfig.Mode`（`ModePropDef`）で参照するため、実行時のプロパティ名解決を行いません。

#### `AppSetting`

アプリケーションのウィンドウ設定とクリップボード連携設定。`SettingsPersistenceBase<AppSetting>` を継承。

- `double WindowWidth` / `double WindowHeight` — ウィンドウサイズ（DIP）
- `double? WindowX` / `double? WindowY` — ウィンドウ左上の位置（画面座標・DIP、null は未保存）
- `bool IsClipboardConvertEnabled` — クリップボード変換（コピーの検知・変換結果の書き戻し）の有効/無効。false の場合はクリップボードを読み書きしません

#### `SettingsPersistenceBase<TSettings>`

設定の JSON ファイルへの自動永続化を提供する基底クラス（`AppSetting` / `ConvertConfig` が継承）。

| メンバー | 型 | 説明 |
| --- | --- | --- |
| `Initialize()` | `void` | 設定ファイルを読み込み、自動保存を開始する |
| `IsAutoSave` | `bool` | 自動保存の有効/無効 |
| `AutoSaveFileName` | `string` | 自動保存先のファイルパス |
| `CancelPendingSave()` | `void` | 保留中の自動保存を取り消す |
| `SaveNow()` | `void` | 保留中の自動保存を取り消し、現在の内容を自動保存先へ**同期で**保存する |
| `SaveToJsonFile(string filePath)` | `void` | 指定パスへ同期で保存する |
| `LoadFromJsonFile(string filePath)` | `void` | 指定パスから読み込んで適用する |

`SaveNow` はアプリの終了時など、デバウンス（300ms）の完了を待てない場面で使用します。
終了直前はプロセスが停止するため、非同期保存では書き込みが完了しません。

### インターフェース（`ClipboardZenHanConverter.Core.Interfaces`）

#### `IClipboardService : IDisposable`

- `event EventHandler<object>? ContentChanged` — クリップボード変更イベント
- `Task<string?> GetTextAsync()` — テキスト取得
- `void SetText(string text)` — テキスト設定
- `void Flush()` — フラッシュ（永続化）

#### `ITextConverter`

- `string Convert(string text)` — テキスト変換

### 変換ロジック（`ClipboardZenHanConverter.Core.Logic`）

#### `CharConverter : ITextConverter, IDisposable`

- `ConvertConfig Config { get; }`
- `string Convert(string text)` — 設定に基づく全角/半角変換とユーザー定義置換
- `ConvertPairs GetConvertPairs()` — 現在の設定に基づく変換ペア生成（キャッシュ）

変換の実行可否（`ConvertConfig.IsEnabledZenHan`）は**参照しません**。変換をスキップするか否かは呼び出し側（画面側）のポリシーです。現在の両アプリ（MewUI 版・WinUI 3 版）は全角/半角変換を常に有効として扱います。

### ウィンドウ位置補正（`ClipboardZenHanConverter.Core.Helpers`）

UI フレームワークに依存しないため Core に配置しています。座標は DIP（`Geometry` の型）で扱います。

#### `WindowPlacement`
| メンバー | 型 | 説明 |
| --- | --- | --- |
| `ClampToVisibleArea(PointD saved, SizeD windowSize, RectD visibleArea)` | `PointD` | 保存されたウィンドウ位置を表示領域内へ補正して返す |

補正内容は次のとおりです。

- 一部が表示領域外: はみ出した辺を表示領域の内側へ詰める
- 完全に表示領域外: 原点（プライマリモニタの左上）へ移動
- ウィンドウが表示領域より大きい: その軸の表示領域先頭へ寄せる

#### `SplitLayout`

上下 2 分割レイアウトの分割比率を扱う純粋ロジック。MewUI 版を除く各 UI のホーム画面が共有します
（MewUI 版はフレームワーク標準の `SplitPanel` が分割を担うため参照しません）。

| メンバー | 型 | 説明 |
| --- | --- | --- |
| `DefaultRatio` | `double`（const） | 既定の分割比率（0.5） |
| `MinRatio` / `MaxRatio` | `double`（const） | 分割比率の下限 0.1 / 上限 0.9 |
| `ClampRatio(double ratio)` | `double` | 比率を有効範囲へ補正する（NaN は既定値） |
| `RatioFromDrag(double startRatio, double deltaY, double trackHeight)` | `double` | ドラッグ量から新しい分割比率を求める |
| `TopWeight(double ratio)` / `BottomWeight(double ratio)` | `double` | 上ペイン / 下ペインの割合（合計 1.0）を求める |

比率（0.0〜1.0）で表すため、UI 側は「上ペイン = 比率」「下ペイン = 1 - 比率」のスターサイズ（相対サイズ）へ
割り当てるだけでよく、**ウィンドウの高さが変化しても上下の比率が自動的に維持されます**。

`trackHeight` は上下 2 ペインの合計高さとし、分割バーの太さは含めません。

### 幾何型（`ClipboardZenHanConverter.Core.Geometry`）

DIP（デバイス非依存ピクセル）座標を表す `readonly record struct` です。物理ピクセルは `Native.PixelBounds` で扱います。

| 型 | メンバー | 説明 |
| --- | --- | --- |
| `PointD` | `X` / `Y` | DIP 座標の点 |
| `SizeD` | `Width` / `Height` | DIP 座標のサイズ |
| `RectD` | `X` / `Y` / `Width` / `Height`、`Left` / `Right` / `Top` / `Bottom` | DIP 座標の矩形 |

マルチモニタ構成では原点より左上のモニタが存在するため、負座標を含みます。

### 変更検出（`ClipboardZenHanConverter.Core.Logic`）

#### `ClipboardChangeDetector`

クリップボードのシーケンス番号を監視し、内容の変更を検出します。UI フレームワーク・タイマーに依存しないため、ポーリング間隔は呼び出し側が決めます。

| メンバー | 型 | 説明 |
| --- | --- | --- |
| `ClipboardChangeDetector(Func<uint> readSequence)` | コンストラクター | シーケンス番号の読み取り関数を指定。構築時の値が初期値になる |
| `HasChanged()` | `bool` | 前回の確認から変化していれば true を返し、内部状態を更新する |
| `RecommendedPollIntervalMs` | `int`（const） | 推奨するポーリング間隔（250 ミリ秒）。各 UI のクリップボード監視タイマーが共有する単一所有元 |

### Win32 API（`ClipboardZenHanConverter.Core.Native`）

UI フレームワークに依存しないため Core に配置しています。`user32.dll` を `LibraryImport`（ソース生成 P/Invoke）で呼び出します。

#### `Win32Clipboard`

| メンバー | 型 | 説明 |
| --- | --- | --- |
| `GetClipboardSequenceNumber()` | `uint` | クリップボードが変更されるたびに増加するシーケンス番号。値の変化で内容変更を検出する |
| `SetText(string text)` | `void` | 指定テキストを `CF_UNICODETEXT` として設定。失敗時は `Win32Exception` |
| `GetText()` | `string?` | クリップボードのテキスト。テキスト形式でない場合は `null` |

#### `Win32Display`

| メンバー | 型 | 説明 |
| --- | --- | --- |
| `GetVirtualScreenBounds()` | `PixelBounds` | 仮想画面（全モニタを包含する外接矩形）を物理ピクセルで取得 |

#### `ScreenVisibleArea`

仮想画面（全モニタの外接矩形）に基づくウィンドウ位置の補正を提供します。
`Win32Display`（物理ピクセルの取得）と `WindowPlacement`（補正ロジック）を組み合わせ、各 UI のウィンドウ復元処理から重複を排除します。

| メンバー | 型 | 説明 |
| --- | --- | --- |
| `GetVisibleArea(double scale)` | `RectD` | 仮想画面の外接矩形を DIP で取得。`scale` が 0 以下なら 100% とみなす |
| `Clamp(PointD saved, SizeD windowSize, double scale)` | `PointD` | 保存されたウィンドウ位置を表示領域内へ補正して返す |
| `ToPhysical(double dip, double scale)` | `int` | DIP を物理ピクセルへ換算（四捨五入）。丸め規則を一元管理する |
| `ToDip(int physical, double scale)` | `double` | 物理ピクセルを DIP へ換算 |

#### `PixelBounds`（`readonly record struct`）

物理ピクセル座標系の矩形。マルチモニタ構成では負座標を含みます。

| メンバー | 型 | 説明 |
| --- | --- | --- |
| `X` / `Y` | `int` | 左上の座標（物理ピクセル） |
| `Width` / `Height` | `int` | 幅・高さ（物理ピクセル） |

DIP への換算は呼び出し側の DPI スケールで行います（Core は DPI スケールを知りません）。

### アイコンデータ（`ClipboardZenHanConverter.Core.Icons`）

UI フレームワークに依存しないため Core に配置しています。SVG ファイルをアイコン形状の唯一の定義元とし、`Icons` フォルダの SVG を埋め込みリソースとして登録します。

#### `FluentIconData`

Fluent Icons の SVG から抽出したパスデータ（`<path>` の `d` 属性の値）を提供します。形状の生成は各 UI フレームワーク側で行います（MewUI は `PathGeometry.Parse`、WinUI3 は `Geometry.Parse` など）。

| メンバー | 型 | 説明 |
| --- | --- | --- |
| `ConvertRange` | `string` | ホーム項目用「Convert Range」アイコンの SVG パスデータ（24px グリッド） |
| `Settings` | `string` | 設定項目用「Settings」アイコンの SVG パスデータ（24px グリッド） |

読み込みは初回アクセスの一度だけ行い、以降は抽出済みの文字列を返却します。
