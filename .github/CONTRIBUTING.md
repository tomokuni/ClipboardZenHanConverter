# 開発者向け情報（ビルド・テスト・CI）

本ドキュメントは ClipboardZenHanConverter を**ビルド・テストする開発者**向けの情報です。
アプリの利用方法は [`../README.md`](../README.md)、各 UI の利用仕様は `../src/app_*/README.md` を参照してください。

- リリース手順（バージョン更新・GitHub Release の作成）は [`RELEASE.md`](RELEASE.md) に分離しています。

## リポジトリ構成

```text
ClipboardZenHanConverter/
├── Directory.Build.props           # 共通プロパティ（LangVersion / Nullable / ImplicitUsings / リリースバージョン）
├── NuGet.config
├── ClipboardZenHanConverter.slnx
├── .github/
│   ├── CONTRIBUTING.md            # 本ドキュメント（開発者向け）
│   ├── RELEASE.md                 # リリース手順
│   ├── workflows/build.yml        # push / PR で復元・ビルド・全テスト（CI）
│   ├── workflows/release.yml      # main への push でバージョン更新・publish・Release 作成
│   └── scripts/bump-version.ps1   # バージョン更新（単一所有元の更新）
├── MewUI_publish_singleaot.bat / MewUI_run_publish.bat    # MewUI 版の単一 exe ビルド/実行（Native AOT）
├── WinUI3_publish_single.bat / WinUI3_run_publish.bat     # WinUI 3 版の単一 exe ビルド/実行（自己完結）
├── AvaloniaUI_publish_aot.bat / AvaloniaUI_run_publish.bat     # Avalonia UI 版の AOT ビルド/実行（Native AOT + ネイティブ DLL）
├── WinForms_publish_single.bat / WinForms_run_publish.bat     # WinForms 版の単一 exe ビルド/実行（自己完結）
├── MewUI_run_release.bat / WinUI3_run_release.bat / AvaloniaUI_run_release.bat / WinForms_run_release.bat
│                                                     # 各版の Release ビルドと実行（publish を行わない軽量な確認用）
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
- リリースバージョン（`<Version>`）は `Directory.Build.props` が単一所有します。各 `.csproj` では指定しません（詳細は [`RELEASE.md`](RELEASE.md)）。
- 復元結果は `project.assets.json`（`obj/` 配下）に記録されます。固定したい場合は `obj/` を削除するか `dotnet restore --force-evaluate` を実行します。

## テスト

```powershell
# ソリューション全体（7 テストプロジェクト、479 件）
dotnet test ClipboardZenHanConverter.slnx

# 個別に実行する場合
dotnet test test\core\tests_core.csproj
dotnet test test\core_presentation\tests_core_presentation.csproj
dotnet test test\app_MewUI\tests_app_MewUI.csproj
dotnet test test\app_WinUI3\tests_app_WinUI3.csproj
dotnet test test\app_AvaloniaUI\tests_app_AvaloniaUI.csproj
dotnet test test\app_WinForms\tests_app_WinForms.csproj
dotnet test test\parity\tests_parity.csproj
```

テストは UI を起動せずに実行できます。  
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

| スクリプト | 対象 | 方式 | 出力先 | 出力物 |
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

| スクリプト | 対象 | 出力先 |
| --- | --- | --- |
| `MewUI_run_release.bat` | MewUI 版 | `src\app_MewUI\bin\Release\net10.0\` |
| `WinUI3_run_release.bat` | WinUI 3 版 | `src\app_WinUI3\bin\Release\net10.0-windows10.0.26100.0\win-x64\` |
| `AvaloniaUI_run_release.bat` | Avalonia UI 版 | `src\app_AvaloniaUI\bin\Release\net10.0\` |
| `WinForms_run_release.bat` | WinForms 版 | `src\app_WinForms\bin\Release\net10.0-windows\` |

- 生成物はフレームワーク依存のため、実行には .NET 10 ランタイム（WinUI 3 版は Windows App SDK を含む）が必要です。
- 配布用の exe が必要な場合は上記の `*_publish_*.bat` を使用してください。

## CI（ビルドとテスト）

[`workflows/build.yml`](workflows/build.yml) が、`main` / `dev` への push と `main` 向け Pull Request のたびに次を実行します。

1. 復元（`dotnet restore ClipboardZenHanConverter.slnx`）
2. Release ビルド（`dotnet build -c Release --no-restore`）
3. 全テスト（`dotnet test -c Release --no-build`。7 テストプロジェクト）

配布物の作成は行いません（[`workflows/release.yml`](workflows/release.yml) が担当します）。`README.md` 先頭の build バッジはこのワークフローの状態を表示します。

publish と同等の条件（クリーンな状態）で事前確認する場合:

```powershell
git clean -xdf -- src test
dotnet restore ClipboardZenHanConverter.slnx
dotnet build ClipboardZenHanConverter.slnx -c Release --no-restore
dotnet test ClipboardZenHanConverter.slnx -c Release --no-build
```
