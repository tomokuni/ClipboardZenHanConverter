namespace EsUtil.ClipboardZenHanConverter.Core.Interfaces;

/// <summary>テキスト変換を実行するインターフェース。</summary>
/// <remarks>ISP（インターフェース分離の原則）に従い、テキスト変換に特化した最小限のインターフェースです。<br/>
/// 実装クラス（CharConverter 等）はこのインターフェースを通じて利用され、<br/>
/// テスト時や別実装への差し替えが容易になります。</remarks>
public interface ITextConverter
{
    /// <summary>テキスト変換を実行します。</summary>
    /// <param name="text">変換対象のテキスト。null の場合は null を返す。</param>
    /// <returns>変換結果のテキスト</returns>
    string Convert(string text);
}
