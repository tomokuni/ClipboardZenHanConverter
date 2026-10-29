# 変更履歴

本プロジェクトの**利用者に影響する変更**を時系列で記録します。 版は人が決めます（自動採番しません）。
リリースの手順は [`.github/RELEASE.md`](.github/RELEASE.md) を参照してください。

## 未リリース（0.2.0）

### 変更（破壊的）

- **配布物（zip）と実行ファイルの名前を `<製品名>.<UI名>` に統一しました**。 旧 `ZenHanConverter.App.<UI名>.exe` は **`ZenHanConverter.<UI名>.exe`** に、zip も `ZenHanConverter.<UI名>.zip` に変わります（例: `ZenHanConverter.App.MewUI.exe` → `ZenHanConverter.MewUI.exe`）。 ショートカットや起動スクリプトで exe 名を指定している場合は変更が必要です。
- 依存パッケージを更新しました（アナライザー・テスト ランナー）。

## 0.1.8（2026-10-28）

### 変更（破壊的）

- **製品名を `ZenHanConverter` に変更しました**（旧 `ClipboardZenHanConverter`）。 配布物（zip）と実行ファイルの名前が変わります。 設定ファイルとログの置き場所も新しい製品名のフォルダになります。

### 修正

- **変換と設定画面の繰り返し処理から、毎回の解析・通知・ファイル読み込みを除きました**（同じ設定を何度も解析せず、変わったときだけ通知します）。
- **プリセットの照合で、全設定の再構築とファイルの読み込みを毎回行っていた**のをやめました（照合用の結果を使い回します）。

### 追加

- **4 つの UI 実装**（Avalonia UI / MewUI / WinForms / WinUI 3）。
- **SBOM（CycloneDX）** を配布物に添付（使った依存を配布物から辿れます）。
- **由来の証明（build provenance）** を配布物に付与（どのワークフロー・どのコミットが作ったかを検証できます）。

## 0.1.7 以前

初回公開（`v0.1.2`）から `v0.1.7` までの変更は、各リリースのノート
（[Releases](https://github.com/tomokuni/ZenHanConverter/releases)）を参照してください。
