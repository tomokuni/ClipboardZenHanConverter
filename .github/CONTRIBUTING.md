# 開発者向け情報（ビルド・テスト・CI）

本ドキュメントは ClipboardZenHanConverter を**ビルド・テストする開発者**向けの情報です。
アプリの利用方法は [`../README.md`](../README.md)、各 UI の利用仕様は `../src/app_*/README.md` を参照してください。

- リリース手順は [`RELEASE.md`](RELEASE.md)、他のリポジトリへの流用方法は [`REUSING.md`](REUSING.md) に分離しています。

## リポジトリ構成

```text
ClipboardZenHanConverter/
├── Directory.Build.props           # 共通プロパティ（LangVersion / Nullable / ImplicitUsings / リリースバージョン）
├── global.json                     # .NET SDK のバージョンとテスト ランナー（ワークフローでは指定しない）
├── ClipboardZenHanConverter.slnx
├── .github/
│   ├── CONTRIBUTING.md            # 本ドキュメント（開発者向け）
│   ├── RELEASE.md                 # リリース手順
│   ├── REUSING.md                 # 他のリポジトリへの流用方法
│   ├── release-config.json        # リリース設定（UI 定義・ソリューション・ブランチ等の単一ソース）
│   ├── dependabot.yml             # GitHub Actions / NuGet の更新 PR
│   ├── actions/read-config/       # release-config.json を読む共通アクション
│   ├── rulesets/tag-version.json  # Tag ruleset の定義（Settings へインポートする）
│   ├── workflows/build.yml        # push / PR でビルド・テスト（配布物は作らない）
│   ├── workflows/publish.yml      # 全 UI の publish（release から呼ばれる共通ワークフロー）
│   ├── workflows/release.yml      # 手動実行で検証・publish・タグ・Release 作成
│   └── scripts/                   # version.ps1（バージョン規則） / set-version.ps1 / verify-release-version.ps1 / show-current-versions.ps1
├── buildScript/             # ビルド・実行スクリプト（リポジトリルートを基準に動作する）
│   ├── MewUI_publish_singleaot.bat / MewUI_run_publish.bat      # MewUI 版の単一 exe ビルド/実行（Native AOT）
│   ├── WinUI3_publish_single.bat / WinUI3_run_publish.bat       # WinUI 3 版の単一 exe ビルド/実行（自己完結）
│   ├── AvaloniaUI_publish_aot.bat / AvaloniaUI_run_publish.bat  # Avalonia UI 版の AOT ビルド/実行（Native AOT + ネイティブ DLL）
│   ├── WinForms_publish_single.bat / WinForms_run_publish.bat   # WinForms 版の単一 exe ビルド/実行（自己完結）
│   └── *_run_release.bat        # 各版の Release ビルドと実行（publish を行わない軽量な確認用）
├── src/
│   ├── app_MewUI/           # MewUI アプリ本体（MewUI 依存）
│   │   ├── README.md / SPEC_ExtFunc.md / SPEC_System.md
│   ├── app_WinUI3/          # WinUI 3 アプリ本体（WinUI 3 依存）
│   │   ├── README.md / SPEC_ExtFunc.md / SPEC_System.md
│   ├── app_AvaloniaUI/      # Avalonia UI アプリ本体（Avalonia UI 依存）
│   │   ├── README.md / SPEC_ExtFunc.md / SPEC_System.md
│   ├── app_WinForms/        # WinForms アプリ本体（Windows Forms 依存）
│   │   ├── README.md / SPEC_ExtFunc.md / SPEC_System.md
│   ├── core/                # 共有コア（モデル・変換ロジック・カテゴリ定義・Win32 API・アイコン。UI 非依存）
│   │   ├── README.md / SPEC_ExtFunc.md / SPEC_System.md
│   └── core_presentation/   # 共有プレゼンテーション層（設定画面の ViewModel。UI 非依存）
│       ├── README.md / SPEC_ExtFunc.md / SPEC_System.md
└── test/                    # 単体テスト（xUnit v3）
    ├── app_MewUI/           # app_MewUI のテスト
    ├── app_WinUI3/          # app_WinUI3 のテスト
    ├── app_AvaloniaUI/      # app_AvaloniaUI のテスト
    ├── app_WinForms/        # app_WinForms のテスト
    ├── core/                # core のテスト（UI フレームワーク非依存）
    ├── core_presentation/   # core_presentation のテスト（UI フレームワーク非依存）
    └── parity/              # アプリ固有 ViewModel の挙動一致を検証（README.md に差異一覧）
```

## プロジェクト

| プロジェクト | 説明 |
| --- | --- |
| `src/core` | 変換ロジック・モデル・変換カテゴリ定義・Win32 API・アイコンデータ（`CharConverter`, `ConvertConfig`, `SegmentDefinitions`, `Win32Clipboard`, `FluentIconData` など）。UI フレームワークに依存しない。 |
| `src/core_presentation` | 4 つの UI が共有する設定画面の ViewModel（`SettingsViewModel`, `ZenHanConvertItem`, `SegmentOption`, `ReplacePairItem`, `PresetEditDialogViewModel`）。UI フレームワークに依存しない。 |
| `src/app_MewUI` | MewUI アプリ本体。クリップボード監視・ホーム/設定画面。 |
| `src/app_WinUI3` | WinUI 3 アプリ本体。同上。 |
| `src/app_AvaloniaUI` | Avalonia UI アプリ本体。同上。 |
| `src/app_WinForms` | Windows Forms アプリ本体。同上。 |
| `test/core` | `src/core` の単体テスト。ソースファイル 1 つにつき `{対象ソースファイル名}Tests.cs` を作成する。 |
| `test/app_MewUI` | `src/app_MewUI` の単体テスト。同上。 |
| `test/app_WinUI3` | `src/app_WinUI3` の単体テスト。同上。 |
| `test/app_AvaloniaUI` | `src/app_AvaloniaUI` の単体テスト。同上。 |
| `test/app_WinForms` | `src/app_WinForms` の単体テスト。同上。 |
| `test/core_presentation` | `src/core_presentation` の単体テスト。 |
| `test/parity` | アプリ固有の ViewModel（`HomeViewModel`）を同じ入力で比較し、挙動の一致を検証する。意図的な差異は `test/parity/README.md` を参照。 |

## ビルド

```powershell
dotnet build ClipboardZenHanConverter.slnx
```

### 名前空間と using

- 名前空間のルートは `EsUtil.ClipboardZenHanConverter` です（例: `EsUtil.ClipboardZenHanConverter.Core.Models`）。
- アセンブリ名・exe 名・設定ファイルの保存先（`%LOCALAPPDATA%\ClipboardZenHanConverter`）は `ClipboardZenHanConverter` のままです。
- 暗黙の global using（`ImplicitUsings`）は無効です。各ソースファイルが使用する名前空間を `using` で明示し、プロジェクト共通の `GlobalUsings.cs` は置きません。

### 依存パッケージ

`PackageReference` には**メジャーバージョンのみ**を指定します（例: `Version="8.*"`）。
同一メジャー内の最新版が復元時に選ばれるため、常に最新の状態でビルドできます。

- メジャーが上がる変更（破壊的変更を含む）は自動では取り込まれません。手動で更新します。
- **浮動指定（`12.*` など）は復元時に同一メジャーの最新版を解決します。** そのため更新が必要になるのはメジャーが上がったときだけです。
  更新の有無は `dotnet list ClipboardZenHanConverter.slnx package --outdated` で確認できます（プレリリースも見る場合は `--include-prerelease`）。
- **推移依存（`SkiaSharp` など）は親パッケージが指定する範囲の最小版が選ばれます。** 直接参照で強制すると
  （例: Avalonia 12 に対する `SkiaSharp` 4 系・`HarfBuzzSharp` 14 系）描画や Native AOT が壊れる恐れがあるため、更新しません。
- テストは **xunit.v3 4 系**（Microsoft.Testing.Platform）を使用します。VSTest 専用の `Microsoft.NET.Test.Sdk` / `xunit.runner.visualstudio` / `coverlet.collector` は参照しません。
- .NET SDK のバージョンとテスト ランナーはリポジトリルートの `global.json` が単一所有します（ワークフローへ `dotnet-version` を書きません。`actions/setup-dotnet` が `global.json` を読みます）。
- リリースバージョン（`<Version>`）は `Directory.Build.props` が単一所有します。自動インクリメントは行わず、リリース時にワークフローが設定します（詳細は [`RELEASE.md`](RELEASE.md)）。
- 復元結果は `project.assets.json`（`obj/` 配下）に記録されます。固定したい場合は `obj/` を削除するか `dotnet restore --force-evaluate` を実行します。

## テスト

テストは UI を起動せずに実行できます。

```powershell
# ソリューション全体（7 テストプロジェクト、449 件）
dotnet test --solution ClipboardZenHanConverter.slnx

# 個別に実行する場合（プロジェクトは --project で指定する）
dotnet test --project test\core\tests_core.csproj
dotnet test --project test\core_presentation\tests_core_presentation.csproj
dotnet test --project test\app_MewUI\tests_app_MewUI.csproj
dotnet test --project test\app_WinUI3\tests_app_WinUI3.csproj
dotnet test --project test\app_AvaloniaUI\tests_app_AvaloniaUI.csproj
dotnet test --project test\app_WinForms\tests_app_WinForms.csproj
dotnet test --project test\parity\tests_parity.csproj
```

**対象の指定には `--solution` / `--project` を使ってください。** xunit.v3 4 系は Microsoft.Testing.Platform（MTP）で
実行され、位置引数（`dotnet test ClipboardZenHanConverter.slnx`）はテスト アプリへの引数として扱われて
**0 件・終了コード 5** になります（実行基盤の指定は `global.json` の `test.runner` が単一所有します）。

**WinUI 3 を参照するテスト（`test/app_WinUI3` / `test/parity`）から WinRT（Windows App SDK）を起動しないでください。**
UI 型の実処理（`DispatcherQueue.GetForCurrentThread()` など）は Windows App SDK の初期化を伴い、`CoreMessagingXP.dll` を
読み込みます。UI を持たない CI（非対話セッション）ではこれが**出力を出さないまま停止**します（Windows App Runtime が
インストール済みのローカルでは再現しません）。UI スレッドへの依存は UI 層（`DependencyInjectionExtensions` が生成する
`IUiDispatcher`）からコンストラクターで受け取り、ViewModel 側で静的に取得しないでください。

**`src/app_WinUI3` は Windows App SDK を自己完結にしています**（`WindowsAppSDKSelfContained=true`）。未設定にすると
アプリのアセンブリにブートストラップの自動初期化（`OnNoMatch_ShowUI`）が埋め込まれ、これを読み込むテストが
Windows App Runtime の無い CI で導入ダイアログ待ちのまま停止します。テスト用プロジェクト側は
`WindowsAppSdkAutoInitialize=false` で自動初期化を無効にしています。

`test/core` は `src/core` のみを参照し、UI フレームワークに依存しないことを保証します。  
`test/core_presentation` は `src/core_presentation` のみを参照し、同じく UI フレームワークに依存しないことを保証します。  
設定ファイルを書き込むテストは、実ユーザーの `%LOCALAPPDATA%` 配下を汚さないよう
自動保存先を一時ディレクトリへ差し替えます。

## 実行

```powershell
# MewUI 版
dotnet run --project src/app_MewUI/app_MewUI.csproj

# WinUI 3 版
dotnet run --project src/app_WinUI3/app_WinUI3.csproj

# Avalonia UI 版
dotnet run --project src/app_AvaloniaUI/app_AvaloniaUI.csproj

# WinForms 版
dotnet run --project src/app_WinForms/app_WinForms.csproj
```

### 配布用ビルド

スクリプトは `buildScript/` に置いてあります。**カレントディレクトリに関係なく**リポジトリルートを基準に動作します。

| スクリプト（`buildScript/` 配下） | 対象 | 方式 | 出力先 | 出力物 |
| --- | --- | --- | --- | --- |
| `MewUI_publish_singleaot.bat` / `MewUI_run_publish.bat` | MewUI 版 | Native AOT | `publish\mewui-<RID>-singleaot\` | exe 1 個（約 12.6 MB） |
| `WinUI3_publish_single.bat` / `WinUI3_run_publish.bat` | WinUI 3 版 | 自己完結（.NET + Windows App SDK） | `publish\winui3-<RID>-single\` | exe 1 個（約 179 MB） |
| `AvaloniaUI_publish_aot.bat` / `AvaloniaUI_run_publish.bat` | Avalonia UI 版 | Native AOT | `publish\avaloniaui-<RID>-aot\` | exe 1 個 + ネイティブ DLL 3 個（約 43.8 MB） |
| `WinForms_publish_single.bat` / `WinForms_run_publish.bat` | WinForms 版 | 自己完結（.NET + WinForms） | `publish\winforms-<RID>-single\` | exe 1 個（約 119 MB） |

- 第 1 引数に RID を渡すと対象を変更できます（既定: `win-x64`）。
- MewUI 版は Native AOT のため完全な単一バイナリです。
- WinUI 3 版は発行時に Windows App SDK の未使用ペイロード（AI / ML / Search / Widgets / WebView2）を自動除外します（約 55 MB 削減。詳細は `src/app_WinUI3/SPEC_System.md`）。
- WinUI 3 版は単一ファイル化できますが、初回起動時に依存ファイルを `%TEMP%\.net\<AppName>\` へ展開します。また WinUI 3 は Native AOT に対応しないため、AOT は使用しません（詳細は `src/app_WinUI3/SPEC_System.md`）。
- Avalonia UI 版は Native AOT ですが、描画に使う SkiaSharp のネイティブ DLL を exe へ同梱できないため、**exe 単体ではなくフォルダー単位で配布**します（`libSkiaSharp.dll` / `av_libglesv2.dll` / `libHarfBuzzSharp.dll` を同じフォルダーに置きます。詳細は `src/app_AvaloniaUI/SPEC_System.md`）。
- WinForms 版は Native AOT に対応せず、トリミングもサポートされないため（`NETSDK1175`）、単一ファイル化のみを行います（詳細は `src/app_WinForms/SPEC_System.md`）。
- WinUI 3 版の exe 名は `ClipboardZenHanConverter.App.WinUI3.exe`、WinForms 版は `ClipboardZenHanConverter.App.WinForms.exe` です（WinUI 3 版はアセンブリ名をプロジェクト名に合わせ、名前空間は `EsUtil.ClipboardZenHanConverter.App.WinUI` のままです）。

### Release ビルドと実行

発行（publish）を行わず、Release 構成のビルドと実行だけを行うスクリプトです。AOT・単一ファイル化を伴わないため短時間で確認できます。

| スクリプト（`buildScript/` 配下） | 対象 | 出力先 |
| --- | --- | --- |
| `MewUI_run_release.bat` | MewUI 版 | `src\app_MewUI\bin\Release\net10.0\` |
| `WinUI3_run_release.bat` | WinUI 3 版 | `src\app_WinUI3\bin\Release\net10.0-windows10.0.26100.0\win-x64\` |
| `AvaloniaUI_run_release.bat` | Avalonia UI 版 | `src\app_AvaloniaUI\bin\Release\net10.0\` |
| `WinForms_run_release.bat` | WinForms 版 | `src\app_WinForms\bin\Release\net10.0-windows\` |

- 生成物はフレームワーク依存のため、実行には .NET 10 ランタイム（WinUI 3 版は Windows App SDK を含む）が必要です。
  - ただし **WinUI 3 版は .NET と Windows App SDK の両方が自己完結**（`SelfContained` / `WindowsAppSDKSelfContained`）のため、
    Windows App Runtime のインストールは不要です（単一 exe 化は行わないため、実行には出力フォルダーごと必要です）。
- 配布用の exe が必要な場合は上記の `*_publish_*.bat` を使用してください。

## CI（ビルドとテスト）

[`workflows/build.yml`](workflows/build.yml) が、`main` / `release/**` への push、`main` 向け Pull Request、手動実行のたびに次を実行します（`windows-latest`）。

1. 復元（`dotnet restore <solutionFile>`）
2. Release ビルド（`dotnet build -c Release --no-restore`）
3. 全テスト（`dotnet test --solution <solutionFile> -c Release --no-build`。7 テストプロジェクト）

- 対象のソリューション ファイルは `release-config.json` の `solutionFile` が単一所有します（ワークフローには書きません）。
- **`dev` への push では実行しません。** `dev` で検証する場合は Actions → Build → **Run workflow**（手動実行）を使用します。
- **配布物は作成しません。** publish（Native AOT を含む重い処理）は、手動実行の [`workflows/release.yml`](workflows/release.yml) が
  [`workflows/publish.yml`](workflows/publish.yml) を通じて**リリース時に 1 度だけ**行います。
- GitHub Release への**登録は行いません**。

この実行結果はリリースの**前提（ゲート）**です。`release.yml` は、リリース対象コミットに対する `build.yml` の成功実行が存在することを確認してから Release を作成します（[`RELEASE.md`](RELEASE.md) を参照）。
`README.md` 先頭の build バッジはこのワークフローの状態を表示します。

### ワークフローの分担

| ワークフロー | 実行契機 | 責務 |
| --- | --- | --- |
| [`workflows/publish.yml`](workflows/publish.yml) | `workflow_call`（`release.yml` からのみ） | 全 UI の publish と保管。**UI の定義と publish 手順の単一実装** |
| [`workflows/build.yml`](workflows/build.yml) | `main` / `release/**` への push、`main` 向け PR、手動 | ビルドとテスト（リリースのゲート） |
| [`workflows/release.yml`](workflows/release.yml) | 手動のみ | リリース（検証 → 全 UI の publish → バージョンコミット → タグ + Release 作成） |

UI を追加・変更する場合は、**`release-config.json` の `uis` を編集するだけ**です（ワークフローの変更は不要）。

publish と同等の条件（クリーンな状態）で事前確認する場合:

```powershell
git clean -xdf -- src test
dotnet restore ClipboardZenHanConverter.slnx
dotnet build ClipboardZenHanConverter.slnx -c Release --no-restore
dotnet test --solution ClipboardZenHanConverter.slnx -c Release --no-build
```
