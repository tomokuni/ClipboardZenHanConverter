namespace ClipboardZenHanConverter.Core.Enums;


/// <summary>全角・半角変換のモードを表す列挙型です。</summary>
/// <remarks>
/// None: 変換なし<br/>
/// ToZen: 半角→全角変換<br/>
/// ToHan: 全角→半角変換<br/>
/// </remarks>
public enum ZenHanMode
{
    /// <summary>変換なし</summary>
    None = 0,
    /// <summary>全角→半角変換</summary>
    ToHan = 1,
    /// <summary>半角→全角変換</summary>
    ToZen = 2,
}


/// <summary>全角･半角カナ変換のモードを表す列挙型です。</summary>
public enum ZenHanKanaMode
{
    /// <summary>変換なし</summary>
    None = 0,
    /// <summary>半角カナに変換</summary>
    ToHan = 1,
    /// <summary>全角カタカナに変換</summary>
    ToZenKata = 2,
    /// <summary>全角ひらがなに変換</summary>
    ToZenHira = 3,
}


/// <summary>全角･半角･Ascii変換のモードを表す列挙型です。</summary>
public enum ZenHanEtcZenHanAsciiMode
{
    /// <summary>変換なし</summary>
    None = 0,
    /// <summary>半角に変換</summary>
    ToHan = 1,
    /// <summary>全角に変換</summary>
    ToZen = 2,
    /// <summary>Asciiに変換</summary>
    ToAscii = 3,
}


/// <summary>バックスラッシュ･円記号変換のモードを表す列挙型です。</summary>
public enum ZenHanEtcYenMode
{
    /// <summary>変換なし</summary>
    None = 0,
    /// <summary>半角バックスラッシュに変換</summary>
    ToHanBSlash = 1,
    /// <summary>全角バックスラッシュに変換</summary>
    ToZenBSlash = 2,
    /// <summary>半角円記号に変換</summary>
    ToHanYen = 3,
    /// <summary>全角円記号に変換</summary>
    ToZenYen = 4,
}


/// <summary>タブとスペース記号、改行コード、連続スペースの変換のモードを表す列挙型です。</summary>
public enum ZenHanEtcSpecial
{
    /// <summary>そのまま</summary>
    None = 0,
    /// <summary>除去</summary>
    Remove = 1,
    /// <summary>半角スペースに変換</summary>
    ToHanSpace = 2,
    /// <summary>全角スペースに変換</summary>
    ToZenSpace = 3,
}
