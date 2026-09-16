# リリース手順（GitHub Actions）

本ドキュメントは ClipboardZenHanConverter の**リリース作業を行う開発者**向けの情報です。
開発環境（ビルド・テスト・CI）は [`CONTRIBUTING.md`](CONTRIBUTING.md)、アプリの利用方法は [`../README.md`](../README.md) を参照してください。

`main` ブランチへ push すると [`workflows/release.yml`](workflows/release.yml) が起動し、バージョンを更新して 4 種のアプリの配布物を GitHub Release として公開します。

## バージョンの単一所有

アセンブリバージョンは `Directory.Build.props` の `<Version>` が単一所有します（各 `.csproj` では指定しません）。
`AssemblyVersion` / `FileVersion` / 表示バージョンはこの値から導出されます。

## main への push 時（自動）

1. バージョンを 1 つ進める（既定は `patch`。`0.1.1` → `0.1.2`）
2. 変更を `github-actions[bot]` として `main` へコミットし、`v0.1.2` のタグを作成
3. 4 種のアプリを Release 構成で publish（`windows-latest`）。**バージョンを更新したコミットのタグ**を対象にするため、配布物とバージョンが一致する
4. 配布物を添付した GitHub Release を作成（リリースノートは前回のタグからの差分を自動生成）

バージョン更新には [`scripts/bump-version.ps1`](scripts/bump-version.ps1) を使用します（ローカルでも実行できます。改行コード・BOM は変更しません）。

- `GITHUB_TOKEN` による push はワークフローを再トリガーしないため、バージョン更新コミットでループしません。
- ドキュメント（`*.md`）のみの変更では起動しません。
- `publish` ジョブは 4 種を並列実行し、いずれかが失敗した場合は Release を作成しません。
- ワークフローが `main` へ push するため、**ブランチ保護**で `github-actions[bot]` の push が拒否される場合は許可設定（または PAT への切り替え）が必要です。

## 手動実行

`Actions` → `Release` → `Run workflow` で、`patch` / `minor` / `major` から上げ幅を選んで実行できます。
手動実行も push と同様にバージョンを 1 つ進めて Release を作成します。

```powershell
# バージョンだけを更新する場合（ワークフローを介さずローカルで実行）
$version = & ./.github/scripts/bump-version.ps1 -Part minor
```

## Release の添付ファイル

| UI | 添付ファイル | 形式 |
| --- | --- | --- |
| MewUI 版 | `ClipboardZenHanConverter.App.MewUI.exe` | 単一 exe（Native AOT） |
| WinUI 3 版 | `ClipboardZenHanConverter.App.WinUI3.exe` | 単一 exe（自己完結） |
| Avalonia UI 版 | `ClipboardZenHanConverter.App.AvaloniaUI.zip` | exe + ネイティブ DLL 3 個 |
| WinForms 版 | `ClipboardZenHanConverter.App.WinForms.exe` | 単一 exe（自己完結） |

publish はリポジトリ直下の `*_publish_*.bat` をそのまま実行するため、ローカルでの配布用ビルドと同一の手順・出力になります。

## 失敗した場合の復旧

- 失敗した実行を「Re-run」しても、**その実行時のワークフロー定義**が使われるため同じ失敗を繰り返します。修正後は `main` へ push するか、手動実行（`Actions` → `Release` → `Run workflow`）を行います。
- `bump` ジョブは成功したが `publish` 以降が失敗した場合、**バージョンとタグだけが消費され Release は未作成**のままになります。その状態で次回実行すると、さらに次のバージョンで Release が作成されます（欠番が生じます）。
