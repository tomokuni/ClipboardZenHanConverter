using System.Runtime.Versioning;

// 対象のアプリ（Windows 専用）を検証するテストである。 対象フレームワークは net10.0 のまま
// （隣のテストと同じ作りを保つ）ため、Windows 10 バージョン 1809 以降を要求することをアセンブリ全体で宣言する。
[assembly: SupportedOSPlatform("windows10.0.17763")]