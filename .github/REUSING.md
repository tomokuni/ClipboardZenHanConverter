# 他のリポジトリへの流用方法

本ドキュメントは、本リポジトリの**ビルド・テスト・リリースの仕組みを別のリポジトリへ展開する**開発者向けの情報です。
仕組みの設計と使い方は [`RELEASE.md`](RELEASE.md)、開発環境は [`CONTRIBUTING.md`](CONTRIBUTING.md) を参照してください。

## 何が流用できるか

**リポジトリ固有の知識を `release-config.json` と `*_publish_*.bat` に閉じ込めています。** それ以外は汎用です。

| ファイル | 流用 | 備考 |
| --- | --- | --- |
| [`release-config.json`](release-config.json) | **コピーして編集** | 唯一のリポジトリ固有設定 |
| [`scripts/version.ps1`](scripts/version.ps1) | **そのまま** | バージョンの規則（リポジトリ非依存） |
| [`scripts/set-version.ps1`](scripts/set-version.ps1) | **そのまま** | バージョンファイルのパスは引数・設定で渡す |
| [`scripts/verify-release-version.ps1`](scripts/verify-release-version.ps1) | **そのまま** | ブランチ規則も汎用 |
| [`workflows/publish.yml`](workflows/publish.yml) | **そのまま** | 設定を読んで UI を publish する |
| [`workflows/release.yml`](workflows/release.yml) | **そのまま** | 設定を読むため変更不要 |
| [`workflows/build.yml`](workflows/build.yml) | **コピーして編集** | ビルド・テストのコマンドのみリポジトリ依存 |
| `*_publish_*.bat` | **新規作成** | UI ごとの publish 手順 |
| 本ドキュメント・`RELEASE.md`・`CONTRIBUTING.md` | コピーして調整 | |

## 手順

### 1. ファイルをコピーする

```text
<新しいリポジトリ>/
├── .github/
│   ├── release-config.json
│   ├── RELEASE.md
│   ├── REUSING.md
│   ├── scripts/
│   │   ├── version.ps1
│   │   ├── set-version.ps1
│   │   └── verify-release-version.ps1
│   └── workflows/
│       ├── publish.yml
│       ├── build.yml
│       └── release.yml
└── <UI ごとの publish スクリプト>.bat
```

### 2. `release-config.json` を編集する

| キー | 内容 | 例 |
| --- | --- | --- |
| `product` | リリース名とアセットのタイトルに使う表示名 | `"ClipboardZenHanConverter"` |
| `versionFile` | バージョンを所有するファイル（リポジトリルートからの相対パス） | `"Directory.Build.props"` |
| `gateWorkflow` | リリースの前提（ゲート）となるワークフローのファイル名 | `"build.yml"` |
| `artifactRetentionDays` | アーティファクトの保持日数 | `30` |
| `releaseNotes` | Release 本文の冒頭に付ける説明 | `"..."` |
| `uis[]` | 配布する UI の定義（下記） | — |

`uis[]` の各要素:

| キー | 内容 |
| --- | --- |
| `name` | アーティファクト名（ジョブの表示にも使う） |
| `script` | publish スクリプト（リポジトリルートからの相対パス） |
| `output` | publish の出力パス（複数ファイルの場合はフォルダ） |
| `asset` | Release に添付するファイル名（zip の場合は作成後の名前） |
| `zip` | 出力がフォルダの場合は `true`（配布前に zip 化する） |

**UI を増やす場合はこの配列に要素を追加するだけです。** ワークフローとスクリプトの変更は不要です。

### 3. `build.yml` を編集する

`build` ジョブのコマンドのみ、リポジトリに合わせて変更します。

```yaml
      - name: 復元
        run: dotnet restore <ソリューション>.slnx

      - name: ビルド（Release）
        run: dotnet build <ソリューション>.slnx -c Release --no-restore

      - name: テスト（Release）
        run: dotnet test <ソリューション>.slnx -c Release --no-build
```

テストが無い・不要な場合はテストのステップを削除します。`publish` の要否判定（バージョン更新のみのコミットをスキップする処理）はそのまま使えます。

### 4. publish スクリプトを用意する

UI ごとに `<名前>_publish_*.bat` を作成します。仕様は次のとおりです。

- リポジトリルートを基準に動作する（`pushd "%~dp0"` でスクリプト位置へ移動する）
- 成功時は終了コード 0、失敗時は非 0 で終了する
- 出力先は `release-config.json` の `output` と一致させる

[AvaloniaUI_publish_aot.bat](../AvaloniaUI_publish_aot.bat) と [MewUI_publish_singleaot.bat](../MewUI_publish_singleaot.bat) が参考になります。

### 5. 動作を確認する

```powershell
# スクリプトの単体確認（バージョン設定・検証）
Copy-Item Directory.Build.props "$env:TEMP/dbp.bak"
& ./.github/scripts/set-version.ps1 -Version 0.0.2
& ./.github/scripts/verify-release-version.ps1 -Version 0.0.3 -Branch main
Copy-Item "$env:TEMP/dbp.bak" Directory.Build.props

# パイプラインの確認（CI と同一条件）
git clean -xdf -- src test
dotnet restore <ソリューション>.slnx
dotnet build <ソリューション>.slnx -c Release --no-restore
dotnet test <ソリューション>.slnx -c Release --no-build
```

その後、`main` へ push して Build が成功することを確認し、`Actions` → `Release` を手動実行します。

## 流用時に必要になる可能性がある変更

| 状況 | 対応 |
| --- | --- |
| **リポジトリが private** | Actions の分数が有料になります。毎 push の publish は実行時間が長いため、`build.yml` の `publish` ジョブを `paths` で限定する（`on.push.paths` に `src/**` を追加する）ことを検討してください |
| **NuGet ギャラリー未公開のパッケージを参照する** | クリーンな CI からは復元できません。リポジトリへ同梱し `NuGet.config` のソースに追加するか、公開してください |
| **複数系列の保守（バックポート）が不要** | `verify-release-version.ps1` のブランチ分岐（`release/X.Y`）はそのままでも害はありませんが、`release/**` のトリガーを `build.yml` から外しても構いません |
| **バージョンを自動で決めたい** | 本仕組みは「人が入力する」前提です。自動化（Conventional Commits からの算出など）を併用する場合は、`verify-release-version.ps1` の検証はそのまま活かせます |
| **Release 以外にも配布したい（NuGet など）** | `release-config.json` に項目を足し、`release.yml` の `release` ジョブにステップを追加します。publish の成果物はアーティファクトとして揃っているため、そのまま公開できます |

## 設計上の約束（変更時に守ること）

流用先でも同じ品質を保つため、次の関係を崩さないでください。

| # | 約束 | 理由 |
| --- | --- | --- |
| 1 | **UI の定義は `release-config.json` だけに置く** | ワークフローへ書き戻すと二重管理になり、追加時に漏れる |
| 2 | **バージョンの規則は `version.ps1` だけに置く** | 正規表現や比較を各所に書くと、判定がずれる |
| 3 | **publish を push より前に実行する** | 失敗時にバージョンとタグを消費しないため |
| 4 | **タグは `gh release create --target` に作成させる** | publish 成功後に初めてタグができる |
| 5 | **バージョンの単一所有元は 1 ファイル** | 番号と成果物の不一致を防ぐ |
