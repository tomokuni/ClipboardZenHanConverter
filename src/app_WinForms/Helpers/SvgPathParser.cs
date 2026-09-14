using System;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace EsUtil.ClipboardZenHanConverter.App.WinForms.Helpers;

/// <summary>SVG のパスデータ（d 属性）を GDI+ の図形パスへ変換します。</summary>
/// <remarks>
/// 提供機能: <br/>
/// - SVG パスコマンド（M / L / H / V / C / S / Q / T / Z と各相対コマンド）の解釈<br/>
/// - GDI+ の <see cref="GraphicsPath"/> への変換<br/><br/>
/// 特徴: <br/>
/// - GDI+ には SVG のパーサが無いため、Fluent Icons の描画に必要な範囲のみを自前で解釈する<br/>
/// - 塗りつぶし規則は EvenOdd（<see cref="FillMode.Alternate"/>）。アイコンの抜き（歯車のリング・中央の輪）は
///   部分パスの重なり回数で表現されているため、この規則で正しく描画できる<br/>
/// - 破線属性・円弧（A / a）には対応しない（Fluent Icons の regular では使用されない）<br/><br/>
/// 注意点: <br/>
/// - 座標は SVG の viewBox（24×24）基準の値としてそのまま格納し、拡大縮小は呼び出し側が行います
/// </remarks>
internal static class SvgPathParser
{
    /// <summary>SVG のパスデータを GDI+ の図形パスへ変換します。</summary>
    /// <param name="pathData">SVG のパスデータ（d 属性の値）。</param>
    /// <returns>変換した図形パス。</returns>
    /// <exception cref="ArgumentException">パスデータが null または空の場合。</exception>
    /// <exception cref="FormatException">数値またはコマンドの並びが不正な場合。</exception>
    /// <exception cref="NotSupportedException">未対応のコマンド（円弧 A / a）が含まれる場合。</exception>
    public static GraphicsPath Parse(string pathData)
    {
        ArgumentException.ThrowIfNullOrEmpty(pathData);

        var reader = new Reader(pathData);
        var path = new GraphicsPath(FillMode.Alternate);

        var x = 0f;
        var y = 0f;
        var startX = 0f;
        var startY = 0f;
        var cubicX = 0f;
        var cubicY = 0f;
        var quadraticX = 0f;
        var quadraticY = 0f;
        var previous = '\0';

        while (reader.HasMore)
        {
            var lastCommand = previous;
            var command = reader.ReadCommand(previous);
            previous = command;

            // 相対コマンドの基準点（現在点）を退避する
            var originX = x;
            var originY = y;

            switch (command)
            {
                case 'M':
                case 'm':
                    x = reader.ReadNumber() + (command == 'm' ? originX : 0);
                    y = reader.ReadNumber() + (command == 'm' ? originY : 0);
                    path.StartFigure();
                    startX = x;
                    startY = y;
                    break;

                case 'L':
                case 'l':
                    x = reader.ReadNumber() + (command == 'l' ? originX : 0);
                    y = reader.ReadNumber() + (command == 'l' ? originY : 0);
                    path.AddLine(originX, originY, x, y);
                    break;

                case 'H':
                case 'h':
                    x = reader.ReadNumber() + (command == 'h' ? originX : 0);
                    path.AddLine(originX, originY, x, y);
                    break;

                case 'V':
                case 'v':
                    y = reader.ReadNumber() + (command == 'v' ? originY : 0);
                    path.AddLine(originX, originY, x, y);
                    break;

                case 'C':
                case 'c':
                {
                    var relative = command == 'c';
                    var c1X = reader.ReadNumber() + (relative ? originX : 0);
                    var c1Y = reader.ReadNumber() + (relative ? originY : 0);
                    var c2X = reader.ReadNumber() + (relative ? originX : 0);
                    var c2Y = reader.ReadNumber() + (relative ? originY : 0);
                    x = reader.ReadNumber() + (relative ? originX : 0);
                    y = reader.ReadNumber() + (relative ? originY : 0);
                    path.AddBezier(originX, originY, c1X, c1Y, c2X, c2Y, x, y);
                    cubicX = c2X;
                    cubicY = c2Y;
                    break;
                }

                case 'S':
                case 's':
                {
                    var relative = command == 's';

                    // 直前が 3 次ベジエでない場合は、制御点を現在点と同一とみなす（SVG の仕様）
                    var isPreviousCubic = lastCommand is 'C' or 'c' or 'S' or 's';
                    var c1X = isPreviousCubic ? (2 * originX) - cubicX : originX;
                    var c1Y = isPreviousCubic ? (2 * originY) - cubicY : originY;

                    var c2X = reader.ReadNumber() + (relative ? originX : 0);
                    var c2Y = reader.ReadNumber() + (relative ? originY : 0);
                    x = reader.ReadNumber() + (relative ? originX : 0);
                    y = reader.ReadNumber() + (relative ? originY : 0);
                    path.AddBezier(originX, originY, c1X, c1Y, c2X, c2Y, x, y);
                    cubicX = c2X;
                    cubicY = c2Y;
                    break;
                }

                case 'Q':
                case 'q':
                {
                    var relative = command == 'q';
                    var cX = reader.ReadNumber() + (relative ? originX : 0);
                    var cY = reader.ReadNumber() + (relative ? originY : 0);
                    x = reader.ReadNumber() + (relative ? originX : 0);
                    y = reader.ReadNumber() + (relative ? originY : 0);
                    path.AddBezier(originX, originY, cX, cY, cX, cY, x, y);
                    quadraticX = cX;
                    quadraticY = cY;
                    break;
                }

                case 'T':
                case 't':
                {
                    var relative = command == 't';

                    // 直前が 2 次ベジエでない場合は、制御点を現在点と同一とみなす（SVG の仕様）
                    var isPreviousQuadratic = lastCommand is 'Q' or 'q' or 'T' or 't';
                    var cX = isPreviousQuadratic ? (2 * originX) - quadraticX : originX;
                    var cY = isPreviousQuadratic ? (2 * originY) - quadraticY : originY;

                    x = reader.ReadNumber() + (relative ? originX : 0);
                    y = reader.ReadNumber() + (relative ? originY : 0);
                    path.AddBezier(originX, originY, cX, cY, cX, cY, x, y);
                    quadraticX = cX;
                    quadraticY = cY;
                    break;
                }

                case 'Z':
                case 'z':
                    path.CloseFigure();
                    x = startX;
                    y = startY;
                    break;

                case 'A':
                case 'a':
                    throw new NotSupportedException("円弧（A / a）コマンドには対応していません。");

                default:
                    throw new FormatException($"未対応の SVG パスコマンドです: {command}");
            }
        }

        return path;
    }

    /// <summary>SVG のパスデータを読み進めるリーダーです。</summary>
    /// <remarks>コマンド文字と数値を交互に読み出します。数値の区切り（空白・カンマ）は自動で読み飛ばします。</remarks>
    private ref struct Reader(string text)
    {
        /// <summary>対象のパスデータ。</summary>
        private readonly string _text = text;

        /// <summary>読み出し位置。</summary>
        private int _index;

        /// <summary>未読の内容が残っているかどうかを取得します。</summary>
        /// <value>区切り文字を除いた未読文字が残っている場合は true。</value>
        public bool HasMore
        {
            get
            {
                SkipSeparators();
                return _index < _text.Length;
            }
        }

        /// <summary>次のコマンド文字を読み出します。</summary>
        /// <param name="previous">直前に読み出したコマンド（暗黙の繰り返しの判定に使用）。</param>
        /// <returns>読み出したコマンド文字。</returns>
        /// <exception cref="FormatException">先頭がコマンド文字でない場合。</exception>
        /// <remarks>コマンド文字が省略されている場合は直前のコマンドの繰り返しとみなします。<br/>
        /// ただし M / m の繰り返しは L / l として扱います（SVG の仕様）。</remarks>
        public char ReadCommand(char previous)
        {
            SkipSeparators();

            if (_index < _text.Length && char.IsAsciiLetter(_text[_index]))
                return _text[_index++];

            return previous switch
            {
                'M' => 'L',
                'm' => 'l',
                '\0' => throw new FormatException("SVG パスデータがコマンド文字で始まっていません。"),
                _ => previous,
            };
        }

        /// <summary>次の数値を読み出します。</summary>
        /// <returns>読み出した数値。</returns>
        /// <exception cref="FormatException">数値として解釈できない場合。</exception>
        public float ReadNumber()
        {
            SkipSeparators();

            var start = _index;
            if (_index < _text.Length && (_text[_index] == '+' || _text[_index] == '-'))
                _index++;

            while (_index < _text.Length && char.IsAsciiDigit(_text[_index]))
                _index++;

            if (_index < _text.Length && _text[_index] == '.')
            {
                _index++;
                while (_index < _text.Length && char.IsAsciiDigit(_text[_index]))
                    _index++;
            }

            if (_index < _text.Length && (_text[_index] == 'e' || _text[_index] == 'E'))
            {
                var exponentStart = _index;
                _index++;
                if (_index < _text.Length && (_text[_index] == '+' || _text[_index] == '-'))
                    _index++;

                if (_index < _text.Length && char.IsAsciiDigit(_text[_index]))
                {
                    while (_index < _text.Length && char.IsAsciiDigit(_text[_index]))
                        _index++;
                }
                else
                {
                    // 指数部として成立しないため読み進めない
                    _index = exponentStart;
                }
            }

            if (start == _index)
                throw new FormatException($"数値を読み出せませんでした（位置 {_index}）。");

            return float.Parse(_text.AsSpan(start, _index - start), NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        /// <summary>空白・改行・カンマなどの区切り文字を読み飛ばします。</summary>
        private void SkipSeparators()
        {
            while (_index < _text.Length)
            {
                var c = _text[_index];
                if (c is ' ' or '\t' or '\r' or '\n' or ',' or '\f')
                {
                    _index++;
                    continue;
                }

                break;
            }
        }
    }
}
