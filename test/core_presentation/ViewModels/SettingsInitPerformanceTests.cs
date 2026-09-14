using ClipboardZenHanConverter.Core.Models;
using System.Diagnostics;
using System.Reflection;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Xunit;

namespace ClipboardZenHanConverter.Tests.Presentation.ViewModels;

/// <summary>設定画面初期化のパフォーマンスを計測するテスト。</summary>
/// <remarks>
/// これらのテストは BenchmarkDotNet の代替として各処理の実行時間を計測し、
/// 初回設定画面遷移の遅延原因を特定することを目的としています。
/// </remarks>
public class SettingsInitPerformanceTests
{
    private readonly ITestOutputHelper _output;

    public SettingsInitPerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>ZenHanConvertItem の静的初期化が走る初回インスタンス生成のコストを計測。</summary>
    /// <remarks>
    /// 静的コンストラクタ内で ConvertConfig の全プロパティ（約50個）に対して
    /// Expression.Compile による getter/setter デリゲート生成が100回実行される。
    /// </remarks>
    [Fact]
    public void FirstZenHanConvertItemCreation_Perf()
    {
        var config = new ConvertConfig();

        var sw = Stopwatch.StartNew();
        var item = new ZenHanConvertItem(config, SegmentDefinitions.NumberDefs[0]);
        sw.Stop();

        _output.WriteLine($"初回 ZenHanConvertItem 生成: {sw.Elapsed.TotalMilliseconds:F1} ms");
        Assert.NotNull(item);
    }

    /// <summary>ZenHanConvertItem の2回目以降（静的コンストラクタ済み）の生成コストを計測。</summary>
    [Fact]
    public void SecondZenHanConvertItemCreation_Perf()
    {
        var config = new ConvertConfig();
        // 1回目で静的コンストラクタを実行
        _ = new ZenHanConvertItem(config, SegmentDefinitions.NumberDefs[0]);

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            _ = new ZenHanConvertItem(config, SegmentDefinitions.NumberDefs[0]);
        }
        sw.Stop();

        _output.WriteLine($"2回目以降 ZenHanConvertItem 生成 (100回平均): {sw.Elapsed.TotalMilliseconds / 100:F3} ms");
    }

    /// <summary>Expression.Compile 全プロパティ分のコストを計測。</summary>
    [Fact]
    public void ExpressionCompileAllProperties_Perf()
    {
        var sw = Stopwatch.StartNew();
        var props = typeof(ConvertConfig).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var pi in props)
        {
            var param = Expression.Parameter(typeof(ConvertConfig), "c");
            _ = Expression.Lambda<Func<ConvertConfig, object?>>(
                Expression.Convert(Expression.Property(param, pi), typeof(object)), param).Compile();

            var valueParam = Expression.Parameter(typeof(object), "v");
            _ = Expression.Lambda<Action<ConvertConfig, object?>>(
                Expression.Assign(
                    Expression.Property(param, pi),
                    Expression.Convert(valueParam, pi.PropertyType)),
                param, valueParam).Compile();
        }
        sw.Stop();

        _output.WriteLine($"Expression.Compile (getter+setter) × {props.Length} プロパティ: {sw.Elapsed.TotalMilliseconds:F1} ms");
    }

    /// <summary>SettingsViewModel コンストラクタ全体のコストを計測。</summary>
    [Fact]
    public void SettingsViewModelConstructor_Perf()
    {
        var config = new ConvertConfig();
        // 事前に静的コンストラクタを実行させておく
        _ = new ZenHanConvertItem(config, SegmentDefinitions.NumberDefs[0]);

        var sw = Stopwatch.StartNew();
        var vm = new SettingsViewModel(config);
        sw.Stop();

        _output.WriteLine($"SettingsViewModel コンストラクタ: {sw.Elapsed.TotalMilliseconds:F1} ms");
        Assert.NotNull(vm);
    }

    /// <summary>SettingsViewModel コンストラクタ（静的初期化含む初回）のコストを計測。</summary>
    [Fact]
    public void SettingsViewModelConstructor_FirstTime_Perf()
    {
        // 新しい AppDomain 相当をシミュレートはできないが、
        // ZenHanConvertItem の静的コンストラクタが未実行の状態を作るため、
        // このテストメソッド内で新規 ConvertConfig を使用する
        var config = new ConvertConfig();

        var sw = Stopwatch.StartNew();
        var vm = new SettingsViewModel(config);
        sw.Stop();

        _output.WriteLine($"SettingsViewModel コンストラクタ (初回): {sw.Elapsed.TotalMilliseconds:F1} ms");
        Assert.NotNull(vm);
    }

    /// <summary>FindMatchingPreset のコストを計測。（プリセットファイルが無い状態）</summary>
    [Fact]
    public void FindMatchingPreset_NoPresetFiles_Perf()
    {
        var config = new ConvertConfig();

        // SettingsViewModel を生成してプリセットリストを初期化
        _ = new SettingsViewModel(config);

        var sw = Stopwatch.StartNew();
        var result = config.FindMatchingPreset();
        sw.Stop();

        _output.WriteLine($"FindMatchingPreset (プリセットファイル無し): {sw.Elapsed.TotalMilliseconds:F1} ms, result={result ?? "null"}");
    }

    /// <summary>ConvertConfig.Initialize のコストを計測。</summary>
    [Fact]
    public void ConvertConfigInitialize_Perf()
    {
        var config = new ConvertConfig();

        var sw = Stopwatch.StartNew();
        config.Initialize();
        sw.Stop();

        _output.WriteLine($"ConvertConfig.Initialize: {sw.Elapsed.TotalMilliseconds:F1} ms");
    }

    /// <summary>SettingsViewModel + ConvertConfig.Initialize のトータルコストを計測。</summary>
    /// <remarks>実際の起動シーケンスに近い形で計測。</remarks>
    [Fact]
    public void TotalInitSequence_Perf()
    {
        var sw = Stopwatch.StartNew();

        // App.OnLaunched のシーケンスを模擬
        var config = new ConvertConfig();
        config.Initialize();
        var vm = new SettingsViewModel(config);

        sw.Stop();

        _output.WriteLine($"トータル初期化シーケンス: {sw.Elapsed.TotalMilliseconds:F1} ms");
    }

    /// <summary>ZenHanConvertItem 静的コンストラクタを RunClassConstructor で強制実行した場合のコスト。</summary>
    [Fact]
    public void RunClassConstructor_Perf()
    {
        var sw = Stopwatch.StartNew();
        RuntimeHelpers.RunClassConstructor(typeof(ZenHanConvertItem).TypeHandle);
        sw.Stop();

        _output.WriteLine($"RunClassConstructor(ZenHanConvertItem): {sw.Elapsed.TotalMilliseconds:F1} ms");
    }
}
