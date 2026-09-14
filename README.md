# ClipboardZenHanConverter

クリップボードのテキストを監視し、全角/半角の自動変換を行う Windows 向けデスクトップアプリケーションです。
`src/core`（モデル・変換ロジック）と `src/core_presentation`（UI 非依存の ViewModel）を共有し、**4 つの UI 実装**から利用します。

| UI | プロジェクト | フレームワーク |
| --- | --- | --- |
| MewUI 版 | `src/app_MewUI` | MewUI（コードファースト・NativeAOT 対応） |
| WinUI 3 版 | `src/app_WinUI3` | WinUI 3（Windows App SDK） |
| Avalonia UI 版 | `src/app_AvaloniaUI` | Avalonia UI（クロスプラットフォーム・XAML） |
| WinForms 版 | `src/app_WinForms` | Windows Forms（コードで画面を構築） |

4 つは同一のコアロジック・変換仕様・設定画面の設定仕様を共有し、**同じ設定ファイル**（`%LOCALAPPDATA%\ClipboardZenHanConverter`）を使います。

## リポジトリ構成

```text
ClipboardZenHanConverter_MewUI/
├── Directory.Build.props
├── NuGet.config
├── ClipboardZenHanConverter_MewUI.slnx
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
    ├── core/                # core のテスト(UI フレ-ムワ-ク非依存)
    ├── core_presentation/   # core_presentation のテスト(UI フレームワーク非依存)
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
dotnet build ClipboardZenHanConverter_MewUI.slnx
```

### 名前空間と using

- 名前空間のルートは `EsUtil.ClipboardZenHanConverter` です（例: `EsUtil.ClipboardZenHanConverter.Core.Models`）。
- アセンブリ名・exe 名・設定ファイルの保存先（`%LOCALAPPDATA%\ClipboardZenHanConverter`）は `ClipboardZenHanConverter` のままです。
- 暗黙の global using（`ImplicitUsings`）は無効です。各ソースファイルが使用する名前空間を `using` で明示し、プロジェクト共通の `GlobalUsings.cs` は置きません。

### 依存パッケージ

`PackageReference` には**メジャーバージョンのみ**を指定します（例: `Version="8.*"`）。
同一メジャー内の最新版が復元時に選ばれるため、常に最新の状態でビルドできます。

- メジャーが上がる変更（破壊的変更を含む）は自動では取り込まれません。手動で更新します。
- `Directory.Build.props` にバージョンを集約せず、各 `.csproj` で指定します。
- 復元結果は `project.assets.json`（`obj/` 配下）に記録されます。固定したい場合は `obj/` を削除するか `dotnet restore --force-evaluate` を実行します。

## テスト

```powershell
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

起動するとホーム画面が表示され、クリップボードにコピーしたテキストを自動変換します。

## リリース（GitHub Actions）

`main` ブランチへ push すると `.github/workflows/release.yml` が起動し、バージョンを更新して 4 種のアプリの配布物を GitHub Release として公開します。

### バージョンの単一所有

アセンブリバージョンは `Directory.Build.props` の `<Version>` が単一所有します（各 `.csproj` では指定しません）。
`AssemblyVersion` / `FileVersion` / 表示バージョンはこの値から導出されます。

### main への push 時（自動）

1. バージョンを 1 つ進める（既定は `patch`。`0.0.1` → `0.0.2`）
2. 変更を `github-actions[bot]` として `main` へコミットし、`v0.0.2` のタグを作成
3. 4 種のアプリを Release 構成で publish（`windows-latest`）
4. 配布物を添付した GitHub Release を作成（リリースノートは前回のタグからの差分を自動生成）

- バージョン更新には `.github/scripts/bump-version.ps1` を使用します（ローカルでも実行できます）。
- `GITHUB_TOKEN` による push はワークフローを再トリガーしないため、バージョン更新コミットでループしません。
- ドキュメント（`*.md`）のみの変更では起動しません。
- `publish` ジョブは 4 種を並列実行し、いずれかが失敗した場合は Release を作成しません。

### 手動実行

`Actions` → `Release` → `Run workflow` で、`patch` / `minor` / `major` から上げ幅を選んで実行できます。

```powershell
# バージョンだけを更新する場合（ワークフローを介さずローカルで実行）
$version = & ./.github/scripts/bump-version.ps1 -Part minor
```

### Release の添付ファイル

| UI | 添付ファイル | 形式 |
| --- | --- | --- |
| MewUI 版 | `ClipboardZenHanConverter.App.MewUI.exe` | 単一 exe（Native AOT） |
| WinUI 3 版 | `ClipboardZenHanConverter.App.WinUI3.exe` | 単一 exe（自己完結） |
| Avalonia UI 版 | `ClipboardZenHanConverter.App.AvaloniaUI.zip` | exe + ネイティブ DLL 3 個 |
| WinForms 版 | `ClipboardZenHanConverter.App.WinForms.exe` | 単一 exe（自己完結） |

publish はリポジトリ直下の `*_publish_*.bat` をそのまま実行するため、ローカルでの配布用ビルドと同一の手順・出力になります。

## 元リポジトリ

WinUI 3 版の実装は `../ClipboardZenHanConverter` にありました。本リポジトリに統合済みです（`src/app_WinUI3`）。
