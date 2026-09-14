using System;

namespace EsUtil.ClipboardZenHanConverter.Core.Logic;

/// <summary>クリップボードのシーケンス番号を監視し、内容の変更を検出するクラス。</summary>
/// <remarks>提供機能: <br/>
/// - 前回確認時からのシーケンス番号の変化による内容変更の検出<br/><br/>
/// 特徴: <br/>
/// - シーケンス番号の読み取りを関数として受け取り、UI フレームワーク・タイマーに依存しない<br/>
/// - ポーリング間隔（タイマー）は呼び出し側が決めるため、ヘッドレスで単体テスト可能</remarks>
/// <param name="readSequence">現在のクリップボードシーケンス番号を取得する関数。</param>
public sealed class ClipboardChangeDetector(Func<uint> readSequence)
{
    /// <summary>推奨するポーリング間隔（ミリ秒）。</summary>
    /// <value>250 ミリ秒。人の操作に対する追従性と CPU 負荷のバランスからこの値とします。</value>
    /// <remarks>各 UI のクリップボード監視タイマーが共有する単一所有元です。</remarks>
    public const int RecommendedPollIntervalMs = 250;

    /// <summary>現在のクリップボードシーケンス番号を取得する関数。</summary>
    private readonly Func<uint> _readSequence = readSequence;

    /// <summary>前回確認時のクリップボードシーケンス番号。</summary>
    private uint _lastSequence = readSequence();

    /// <summary>前回の確認からクリップボードの内容が変化したかを判定します。</summary>
    /// <returns>変化している場合は true。</returns>
    /// <remarks>処理フロー: <br/>
    /// 1. 現在のシーケンス番号を取得<br/>
    /// 2. 前回値と同じ場合は false を返却（状態は更新しない）<br/>
    /// 3. 異なる場合は前回値を更新して true を返却<br/><br/>
    /// 注意点: <br/>
    /// - 呼び出しは同一スレッドから行ってください（内部状態を排他制御しません）</remarks>
    public bool HasChanged()
    {
        var current = _readSequence();
        if (current == _lastSequence)
            return false;

        _lastSequence = current;
        return true;
    }
}
