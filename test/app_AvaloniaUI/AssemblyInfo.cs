using Xunit;

// プリセットの保存先（ConvertConfig.PresetDirectory）はプロセス全体で共有される静的状態のため、
// プリセットを扱うテストが並列実行されると互いのファイルを消し合います。
// テスト間の干渉を避けるため、このアセンブリでは並列実行を無効にします。
[assembly: CollectionBehavior(DisableTestParallelization = true)]
