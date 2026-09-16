<!-- markdownlint-disable MD041 -- バッジを先頭に配置するため -->
[![release](https://img.shields.io/github/v/release/tomokuni/ClipboardZenHanConverter?label=release)](https://github.com/tomokuni/ClipboardZenHanConverter/releases)
[![build](https://github.com/tomokuni/ClipboardZenHanConverter/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/tomokuni/ClipboardZenHanConverter/actions/workflows/build.yml)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![platform](https://img.shields.io/badge/platform-Windows-0078D4?logo=windows)](https://www.microsoft.com/windows)

# ClipboardZenHanConverter

クリップボードのテキストを監視し、全角/半角の自動変換を行う Windows 向けデスクトップアプリケーションです。
`src/core`（モデル・変換ロジック）と `src/core_presentation`（UI 非依存の ViewModel）を共有し、**4 つの UI 実装**から利用します。

| UI | プロジェクト | フレームワーク | 利用仕様書 |
| --- | --- | --- | --- |
| MewUI 版 | `src/app_MewUI` | MewUI（コードファースト・Native AOT 対応） | [`README.md`](src/app_MewUI/README.md) |
| WinUI 3 版 | `src/app_WinUI3` | WinUI 3（Windows App SDK） | [`README.md`](src/app_WinUI3/README.md) |
| Avalonia UI 版 | `src/app_AvaloniaUI` | Avalonia UI（クロスプラットフォーム・XAML） | [`README.md`](src/app_AvaloniaUI/README.md) |
| WinForms 版 | `src/app_WinForms` | Windows Forms（コードで画面を構築） | [`README.md`](src/app_WinForms/README.md) |

4 つは同一のコアロジック・変換仕様・設定画面の設定仕様を共有し、**同じ設定ファイル**を使います。

## 入手方法

[GitHub Releases](https://github.com/tomokuni/ClipboardZenHanConverter/releases) から最新版をダウンロードします。
いずれも自己完結で、.NET ランタイムのインストールは不要です。

| UI | ファイル | 形式 |
| --- | --- | --- |
| MewUI 版 | `ClipboardZenHanConverter.App.MewUI.exe` | 単一 exe（Native AOT） |
| WinUI 3 版 | `ClipboardZenHanConverter.App.WinUI3.exe` | 単一 exe（自己完結） |
| Avalonia UI 版 | `ClipboardZenHanConverter.App.AvaloniaUI.zip` | exe + ネイティブ DLL 3 個 |
| WinForms 版 | `ClipboardZenHanConverter.App.WinForms.exe` | 単一 exe（自己完結） |

- Avalonia UI 版は exe 単体では起動しません。zip を展開し、同じフォルダーにあるネイティブ DLL と一緒に使用します。
- 起動するとホーム画面が表示され、クリップボードにコピーしたテキストを自動変換します。

## 設定ファイル

4 つの UI は同じ設定ファイルを共有するため、UI を切り替えても設定は引き継がれます。

| ファイル | 内容 |
| --- | --- |
| `%LOCALAPPDATA%\ClipboardZenHanConverter\AppSetting.json` | ウィンドウ位置・サイズ、クリップボード変換の有効/無効 |
| `%LOCALAPPDATA%\ClipboardZenHanConverter\Settings.json` | 全角/半角の変換設定、置換ルール |
| `%LOCALAPPDATA%\ClipboardZenHanConverter\Presets\{名前}.json` | ユーザー定義プリセット |

設定は各 UI の設定画面から変更でき、終了時に自動保存されます。

## ドキュメント

| ドキュメント | 内容 |
| --- | --- |
| `src/app_*/README.md` | 各 UI の利用仕様書（機能・画面構成・使い方） |
| `src/app_*/SPEC_ExtFunc.md` | 各 UI の外部機能仕様書（公開 API） |
| `src/app_*/SPEC_System.md` | 各 UI のシステム仕様書（構成・動作仕様） |
| `src/core/` / `src/core_presentation/` の README・SPEC | 共有コア（変換ロジック・モデル）と共有プレゼンテーション層（設定画面 ViewModel）の仕様 |

## 開発者向け情報

ビルド・テスト・CI・リリースは開発者向けドキュメントに分離しています。

- [`.github/CONTRIBUTING.md`](.github/CONTRIBUTING.md) — リポジトリ構成、プロジェクト、ビルド、テスト、実行、配布用ビルド、CI
- [`.github/RELEASE.md`](.github/RELEASE.md) — バージョンの単一所有、リリースワークフロー、手動実行、添付ファイル

## 元リポジトリ

WinUI 3 版の実装は `../ClipboardZenHanConverter` にありました。本リポジトリに統合済みです（`src/app_WinUI3`）。
