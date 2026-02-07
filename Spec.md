# クラス詳細仕様書 (ClipboardZenHanConverter)

このドキュメントは、本アプリケーションの主要クラスの詳細仕様を記述したものです。
AIモデルにコード生成を依頼する際のプロンプトとしても利用可能です。

## 対象クラス

### 1. CharConverter (Core.Logic)
文字列変換の中核ロジックを担当するクラス。

- **役割**: `ConvertConfig` の設定に基づき、入力文字列に対して変換ルールを適用する。
- **実装詳細**:
    - `IDisposable` を実装し、`ConvertConfig.PropertyChanged` イベントの購読解除を行う。
    - `GetConvertPairs()` メソッドにより、設定値から `ConvertPairs` オブジェクトを生成する。この計算コストを抑えるため、`_cachedPairs` フィールドに結果をキャッシュする。設定変更時にキャッシュは破棄される。
    - 変換処理は `Convert(string text)` メソッドで行う。
        1. `ZenHanConverter.ToNormalize(text)` で正規化。
        2. 生成された `ConvertPairs` を用いて文字置換。
        3. `SingleSpaceRegex` (Source Generator) を用いて連続スペースの処理を行う。
- **依存関係**: `ConvertConfig`, `ConvertPairs` (EsUtil), `ZenHanMode` (Enums)

### 2. ZenHanConvertItem (ViewModels)
設定画面の各項目を ViewModel 化したクラス。

- **役割**: `ConvertConfig` の特定のプロパティと、画面上の選択肢 (`SegmentItem`) をバインディングするための中間層。
- **実装詳細**:
    - Primary Constructor を使用: `(ConvertConfig config, SegmentDefine def)`.
    - リフレクション (`PropertyInfo`) を使用して、`SegmentDefine.Prop` で指定された `ConvertConfig` のプロパティにアクセスする。
    - `SelectedLabel` プロパティの setter で、リフレクションを用いて設定値を更新する。
    - `Segments` 配列を持ち、選択肢を管理する。
- **パフォーマンス**:
    - コンストラクタで `PropertyInfo` を一度だけ取得し、以降は再利用する。

### 3. SettingsViewModel (ViewModels)
設定画面全体の ViewModel。

- **役割**: 各種 `ZenHanConvertItem` のコレクションを保持し、View に公開する。
- **実装詳細**:
    - `SettingsModel` から定義 (`SegmentDefine`) を読み込み、`ZenHanConvertItem` のインスタンスを生成する。
    - Collection Expression (`[...]`) を用いて簡潔に初期化記述を行っている。

### 4. EnableStyleSelector (Core.Helpers)
リスト項目の有効/無効スタイル切り替えロジック。

- **役割**: `StyleSelector` を継承し、`SegmentItem.IsEnabled` に基づいてスタイルを振り分ける。
- **実装詳細**:
    - パターンマッチング `is SegmentItem segment` を使用して型安全に判定。
    - `EnabledStyle` と `DisabledStyle` プロパティを持つ。

## パフォーマンス向上施策の詳細
- **リフレクションのキャッシュ**: `ZenHanConvertItem` において `PropertyInfo` をインスタンス生成時にキャッシュし、プロパティアクセスのオーバーヘッドを最小化。
- **イベント購読の管理**: `CharConverter` や `HomeViewModel` で `IDisposable` を実装し、メモリリークを防止。
- **正規表現の事前コンパイル**: `[GeneratedRegex]` を使用し、コンパイル時または初回実行時に最適化されたコードを生成。
- **キャッシュ機構**: `CharConverter` クラス内で変換ペアのマップ (`ConvertPairs`) をキャッシュし、設定変更時のみ再生成することで、変換処理ごとのインスタンス生成コストを削減しています。
- **Regex Source Generator**: `.NET 7` 以降の `[GeneratedRegex]` を使用し、正規表現エンジンの初期化コストと実行時間を最適化しています。
- **Span / Memory**: 内部的な文字列処理において、`Span<char>` を活用したアロケーション削減を行っています。
- **非同期処理**: クリップボード操作 (`IClipboardService`) を非同期で行い、UIスレッドのブロックを防いでいます。
- **リフレクションの最適化**: ViewModel層でのプロパティアクセスにおいて `PropertyInfo` をキャッシュし、オーバーヘッドを最小限に抑えています。
