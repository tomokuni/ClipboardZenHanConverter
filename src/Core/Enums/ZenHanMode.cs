namespace EsUtil.ClipboardZenHanConverter.Core.Enums;

/// <summary>全角/半角変換モードを表します。</summary>
/// <remarks>基本的な文字種別ごとの変換方向を指定します。</remarks>
public enum ZenHanMode
{
    /// <summary>変換なし</summary>
    None = 0,
    /// <summary>全角文字を半角に変換</summary>
    ToHan = 1,
    /// <summary>半角文字を全角に変換</summary>
    ToZen = 2,
}

/// <summary>半角カナ/全角カタカナ/全角ひらがなの変換モードを表します。</summary>
/// <remarks>半角カナ、全角カタカナ、全角ひらがなの3系統間の相互変換方向を指定します。</remarks>
public enum ZenHanKanaMode
{
    /// <summary>変換なし</summary>
    None = 0,
    /// <summary>全角かなを半角カナに変換</summary>
    ToHan = 1,
    /// <summary>半角カナ/全角ひらがなを全角カタカナに変換</summary>
    ToZenKata = 2,
    /// <summary>半角カナ/全角カタカナを全角ひらがなに変換</summary>
    ToZenHira = 3,
}

/// <summary>全角/半角/ASCII 変換モードを表します。</summary>
/// <remarks>長音記号、読点、句点のかな約物に使用します。<br/>
/// 3系統（全角・半角・ASCII）間の変換方向を指定します。</remarks>
public enum ZenHanEtcZenHanAsciiMode
{
    /// <summary>変換なし</summary>
    None = 0,
    /// <summary>全角文字を半角に変換</summary>
    ToHan = 1,
    /// <summary>半角文字を全角に変換</summary>
    ToZen = 2,
    /// <summary>文字を対応するASCII文字に変換（例: ー → -）</summary>
    ToAscii = 3,
}

/// <summary>円記号/バックスラッシュの変換モードを表します。</summary>
/// <remarks>半角バックスラッシュ（\）、全角バックスラッシュ（＼）、<br/>
/// 半角円記号（¥）、全角円記号（￥）の4系統間の変換方向を指定します。</remarks>
public enum ZenHanEtcYenMode
{
    /// <summary>変換なし</summary>
    None = 0,
    /// <summary>半角バックスラッシュ（\）に変換</summary>
    ToHanBSlash = 1,
    /// <summary>全角バックスラッシュ（＼）に変換</summary>
    ToZenBSlash = 2,
    /// <summary>半角円記号（¥）に変換</summary>
    ToHanYen = 3,
    /// <summary>全角円記号（￥）に変換</summary>
    ToZenYen = 4,
}

/// <summary>特殊文字（タブ・改行・スペース）の変換モードを表します。</summary>
/// <remarks>タブ文字、改行文字、連続スペースの整形方法を指定します。<br/>
/// 除去、半角スペース化、全角スペース化が選択可能です。</remarks>
public enum ZenHanEtcSpecial
{
    /// <summary>変換なし（そのまま保持）</summary>
    None = 0,
    /// <summary>該当文字を除去</summary>
    Remove = 1,
    /// <summary>半角スペースに変換</summary>
    ToHanSpace = 2,
    /// <summary>全角スペースに変換</summary>
    ToZenSpace = 3,
}
