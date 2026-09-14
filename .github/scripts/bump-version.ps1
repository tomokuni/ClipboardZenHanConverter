<#
.SYNOPSIS
Directory.Build.props が単一所有するリリースバージョンをインクリメントする。
.DESCRIPTION
バージョンの単一所有元は Directory.Build.props の <Version> のみである。
本スクリプトはその 1 行を書き換え、更新後のバージョン文字列を出力する。
改行コードと BOM の有無を変えないよう、テキストをそのまま読み書きする。
ローカル実行と GitHub Actions（.github/workflows/release.yml）の両方から使用する。
.PARAMETER Part
インクリメントする桁。major / minor / patch のいずれか（既定は patch）。
上位桁の更新時は下位桁を 0 に戻す。
.OUTPUTS
System.String
更新後のバージョン文字列（例: 0.0.2）。
.EXAMPLE
$version = & ./.github/scripts/bump-version.ps1 -Part minor
#>
[CmdletBinding()]
param(
    [ValidateSet('major', 'minor', 'patch')]
    [string]$Part = 'patch'
)

$ErrorActionPreference = 'Stop'

# 本スクリプトは <リポジトリルート>/.github/scripts に置かれている
$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$propsPath = Join-Path $repoRoot 'Directory.Build.props'

if (-not (Test-Path -LiteralPath $propsPath)) {
    throw "Directory.Build.props が見つかりません: $propsPath"
}

$pattern = '<Version>(\d+)\.(\d+)\.(\d+)</Version>'
$text = [System.IO.File]::ReadAllText($propsPath, [System.Text.Encoding]::UTF8)
$match = [regex]::Match($text, $pattern)

if (-not $match.Success) {
    throw "Directory.Build.props に <Version>Major.Minor.Patch</Version> がありません: $propsPath"
}

$major = [int]$match.Groups[1].Value
$minor = [int]$match.Groups[2].Value
$patch = [int]$match.Groups[3].Value

switch ($Part) {
    'major' {
        $major++
        $minor = 0
        $patch = 0
    }
    'minor' {
        $minor++
        $patch = 0
    }
    'patch' {
        $patch++
    }
}

$version = "$major.$minor.$patch"
$updated = $text.Substring(0, $match.Index) + "<Version>$version</Version>" + $text.Substring($match.Index + $match.Length)

[System.IO.File]::WriteAllText($propsPath, $updated, [System.Text.UTF8Encoding]::new($false))

Write-Host "バージョンを更新しました: $($match.Groups[0].Value) -> <Version>$version</Version> ($Part)"

# 呼び出し側（GitHub Actions）が取り出せるよう、バージョン文字列のみを出力する
Write-Output $version
