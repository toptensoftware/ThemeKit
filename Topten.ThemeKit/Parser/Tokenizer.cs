using System;
using System.Collections.Generic;
using System.Text;

namespace Topten.ThemeKit
{
    internal enum Token
    {
        EOF,

        Identifier,
        Literal,
        ColorLiteral,  // In 0xAARRGGBB format in TokenLiteral
        
        Comma,
        Period,
        Colon,
        SemiColon,
        OpenRound,
        CloseRound,
        OpenSquare,
        CloseSquare,
        OpenBrace,
        CloseBrace,

        Plus,
        Minus,
        Multiply,
        Divide,
        Modulus,

        Assign,

        Question,

        LogicalAnd,
        LogicalOr,
        LogicalNot,

        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
        BitwiseComplement,

        Shl,
        Shr,

        LT,
        GT,
        LE,
        GE,
        NE,
        EQ,
        Unknown,
    }

    internal class Tokenizer
    {
        public Tokenizer(StringSource source)
        {
            _source = source;
            Next();
        }

        Dictionary<string, Token> _keywordMap = new Dictionary<string, Token>();

        StringSource _source;

        Token _token;
        object _literal;
        int _tokenPos;

        public Token Token => _token;
        public SourcePosition TokenPosition => _source.CreatePosition(_tokenPos);
        public string TokenRaw => _source.Extract(_tokenPos);
        public object TokenLiteral => _literal;
        public string TokenIdentifier
        {
            get
            {
                System.Diagnostics.Debug.Assert(Token == Token.Identifier);
                return (string)_literal;
            }
        }

        public void Rewind(SourcePosition pos)
        {
            // Can't switch sources on rewind
            System.Diagnostics.Debug.Assert(pos.Source == _source);

            // Rewind and load the next token
            _source.Position = pos.Position;
            Next();
        }

        public void Next()
        {
            _token = GetNextToken();
            return;
        }

        public StringSource Source => _source;

        public bool IsIdentifier(string str)
        {
            return _token == Token.Identifier && string.Compare(str, (string)_literal, true) == 0;
        }

        public bool TrySkipIdentifier(string str)
        {
            if (IsIdentifier(str))
            {
                Next();
                return true;
            }

            return false;
        }

        public void SkipIdentifier(string str)
        {
            if (IsIdentifier(str))
            {
                Next();
                return;
            }

            throw new CodeException($"syntax error: expected '{str}', found '{TokenRaw}'", TokenPosition);
        }


        public bool TrySkipToken(Token token)
        {
            if (_token == token)
            {
                Next();
                return true;
            }
            return false;
        }

        public void CheckToken(Token token, string suffix = null)
        {
            if (_token != token)
            {
                if (suffix != null)
                    throw new CodeException($"syntax error: expected {DescribeToken(token)} {suffix}, found {DescribeToken(Token, TokenRaw)}", TokenPosition);
                else
                    throw new CodeException($"syntax error: expected {DescribeToken(token)}, found {DescribeToken(Token, TokenRaw)}", TokenPosition);
            }
        }

        public void SkipToken(Token token)
        {
            CheckToken(token);
            Next();
        }

        public CodeException Unexpected()
        {
            return new CodeException($"syntax error: unexpected: {DescribeToken(Token, TokenRaw)}", TokenPosition);
        }

        public CodeException Unexpected(string expected)
        {
            return new CodeException($"syntax error: {DescribeToken(Token, TokenRaw)}, expected {expected}", TokenPosition);
        }

        public static string DescribeToken(Token token, string raw = null)
        {
            switch (token)
            {
                case Token.EOF: return "end of file";
                case Token.Identifier: return $"identifier '{raw}'";
                case Token.ColorLiteral: return $"literal color '{raw}'";
                case Token.Literal: return $"literal '{raw}'";
                case Token.Comma: return "','";
                case Token.SemiColon: return "';'";
                case Token.Period: return "'.'";
                case Token.Colon: return "':'";
                case Token.OpenRound: return "'('";
                case Token.CloseRound: return "')'";
                case Token.OpenSquare: return "'['";
                case Token.CloseSquare: return "']'";
                case Token.OpenBrace: return "'{'";
                case Token.CloseBrace: return "'}'";
                case Token.Plus: return "'+'";
                case Token.Minus: return "'-'";
                case Token.Multiply: return "'*'";
                case Token.Divide: return "'/'";
                case Token.Modulus: return "'%'";
                case Token.Assign: return "'-'";
                case Token.Question: return "'?'";
                case Token.LogicalAnd: return "&&";
                case Token.LogicalOr: return "'||'";
                case Token.LogicalNot: return "'!'";
                case Token.BitwiseAnd: return "'%";
                case Token.BitwiseOr: return "|";
                case Token.BitwiseXor: return "^";
                case Token.BitwiseComplement: return "~";
                case Token.Shl: return "<<";
                case Token.Shr: return ">>";
                case Token.LT: return "<";
                case Token.GT: return ">";
                case Token.LE: return "<=";
                case Token.GE: return ">=";
                case Token.NE: return "!=";
                case Token.EQ: return "==";
            }

            if (raw != null)
                return $"unknown token: '{raw}'";
            else
                return "unknown token";
        }

        // Set to true to disable << and >> tokens and instead
        // return multiple Token.LT and Token.GT
        public bool ParsingType
        {
            get;
            set;
        }

        Token GetNextToken()
        {
            try_again:

            // Skip any linespace
            _source.SkipWhitespace();

            // Capture the position of the current token
            _tokenPos = _source.Position;

            // C++ Style Comment
            if (_source.Skip("//"))
            {
                _source.SkipToEOL();
                goto try_again;
            }

            // Block Comment
            if (_source.Skip("/*"))
            {
                if (!_source.SkipUntil("*/"))
                    throw new CodeException("syntax error: unclosed block comment", TokenPosition);
                _source.Skip("*/");
                goto try_again;
            }

            // Capture the position of the current token
            _tokenPos = _source.Position;

            if (_source.EOF)
                return Token.EOF;

            // Identifier?
            if (IsIdentifierLeadChar(_source.Current))
            {
                _literal = _source.SkipAndExtract(IsIdentifierChar);

                // Named literals?
                switch ((string)_literal)
                {
                    case "true":
                        _literal = true;
                        return Token.Literal;

                    case "false":
                        _literal = false;
                        return Token.Literal;

                    case "null":
                        _literal = null;
                        return Token.Literal;
                }

                if (_keywordMap.TryGetValue((string)_literal, out var token))
                {
                    return token;
                }

                return Token.Identifier;
            }

            // Color
            if (_source.Current == '#')
            {
                _source.Next();
                var colorString = _source.SkipAndExtract(IsHexDigit);
                switch (colorString.Length)
                {
                    case 3:
                        colorString = $"FF{colorString[0]}{colorString[0]}{colorString[1]}{colorString[1]}{colorString[2]}{colorString[2]}";
                        break;

                    case 6:
                        colorString = "FF" + colorString;
                        break;

                    case 8:
                        break;

                    default:
                        throw new CodeException("Invalid color specifier, expected 3, 6 or 8 hex digits", TokenPosition);
                }

                _literal = Convert.ToUInt32(colorString, 16);
                return Token.ColorLiteral;
            }

            // Number?
            if (IsDigit(_source.Current))
            {
                _literal = ParseNumber();
                return Token.Literal;
            }

            // Other characters
            switch (_source.Current)
            {
                case ',':
                    _source.Next();
                    return Token.Comma;

                case '.':
                    _source.Next();
                    return Token.Period;

                case ':':
                    _source.Next();
                    return Token.Colon;

                case ';':
                    _source.Next();
                    return Token.SemiColon;

                case '(':
                    _source.Next();
                    return Token.OpenRound;

                case ')':
                    _source.Next();
                    return Token.CloseRound;

                case '[':
                    _source.Next();
                    return Token.OpenSquare;

                case ']':
                    _source.Next();
                    return Token.CloseSquare;

                case '{':
                    _source.Next();
                    return Token.OpenBrace;

                case '}':
                    _source.Next();
                    return Token.CloseBrace;

                case '+':
                    _source.Next();
                    return Token.Plus;

                case '-':
                    _source.Next();
                    return Token.Minus;

                case '*':
                    _source.Next();
                    return Token.Multiply;

                case '/':
                    _source.Next();
                    return Token.Divide;

                case '%':
                    _source.Next();
                    return Token.Modulus;

                case '&':
                    _source.Next();
                    if (_source.Current == '&')
                    {
                        _source.Next();
                        return Token.LogicalAnd;
                    }
                    return Token.BitwiseAnd;

                case '|':
                    _source.Next();
                    if (_source.Current == '|')
                    {
                        _source.Next();
                        return Token.LogicalOr;
                    }
                    return Token.BitwiseOr;

                case '!':
                    _source.Next();
                    if (_source.Skip('='))
                        return Token.NE;
                    return Token.LogicalNot;

                case '^':
                    _source.Next();
                    return Token.BitwiseXor;

                case '~':
                    _source.Next();
                    return Token.BitwiseComplement;

                case '<':
                    _source.Next();
                    if (!ParsingType && _source.Skip('<'))
                        return Token.Shl;
                    if (_source.Skip('='))
                        return Token.LE;
                    return Token.LT;

                case '>':
                    _source.Next();
                    if (!ParsingType && _source.Skip('>'))
                        return Token.Shr;
                    if (_source.Skip('='))
                        return Token.GE;
                    return Token.GT;

                case '=':
                    _source.Next();
                    if (_source.Skip('='))
                        return Token.EQ;
                    return Token.Assign;

                case '?':
                    _source.Next();
                    return Token.Question;

                case '\"':
                    _literal = SkipString();
                    return Token.Literal;
            }

            _source.Next();
            return Token.Unknown;
        }

        StringBuilder _sb = new StringBuilder();

        string SkipString()
        {
            // Skip the delimiter
            var delim = _source.Current;
            _source.Next();

            // Reset working buffer
            _sb.Length = 0;

            // process characters
            while (!_source.EOF && !_source.EOL)
            {
                // Escape sequence?
                if (_source.Current == '\\')
                {
                    _source.Next();
                    var escape = _source.Current;
                    switch (escape)
                    {
                        case '\"': _sb.Append('\"'); break;
                        case '\\': _sb.Append('\\'); break;
                        case 'b': _sb.Append('\b'); break;
                        case 'f': _sb.Append('\f'); break;
                        case 'n': _sb.Append('\n'); break;
                        case 'r': _sb.Append('\r'); break;
                        case 't': _sb.Append('\t'); break;
                        case 'u':
                            var sbHex = new StringBuilder();
                            for (int i = 0; i < 4; i++)
                            {
                                _source.Next();
                                sbHex.Append(_source.Current);
                            }
                            _sb.Append((char)Convert.ToUInt16(sbHex.ToString(), 16));
                            break;

                        default:
                            _sb.Append("\\");
                            _sb.Append(escape);
                            break;
                    }
                }
                else if (_source.Current == delim)
                {
                    // End of string
                    _source.Next();
                    return _sb.ToString();
                }
                else
                {
                    // Other character
                    _sb.Append(_source.Current);
                }

                // Next
                _source.Next();
            }

            throw new CodeException("syntax error: unterminated string literal", _source.CapturePosition()); ;
        }

        int ParseVectorWidth()
        {
            var pos = _source.CapturePosition();
            var str = _source.SkipAndExtract(IsDigit);
            if (str == null)
                throw new CodeException("syntax error: missing bit vector suffix width", pos);
            return int.Parse(str);
        }

        object ParseNumber()
        {
            string number = "";
            object value;
            var floatingPoint = false;
            try
            {
                if (_source.SkipI("0x"))
                {
                    // Hex number
                    number = _source.SkipAndExtract(x => IsHexDigit(x) || x == '_').Replace("_", "");
                    value = Convert.ToUInt64(number, 16);
                }
                else if (_source.SkipI("0b"))
                {
                    // Binary
                    number = _source.SkipAndExtract(x => IsBinaryDigit(x) || x == '_').Replace("_", "");
                    value = Convert.ToUInt64(number, 2);
                }
                else
                {
                    // Decimal number
                    var pos = _source.Position;
                    _source.Skip(x => IsDigit(x) || x == '_');
                    if (_source.Current == '.' && _source.CharAt(1) != '.')     // Don't fall for '..'  eg: 7..0
                    {
                        _source.Next();
                        floatingPoint = true;
                        _source.Skip(x => IsDigit(x) || x == '_');
                    }
                    if (_source.SkipI('e'))
                    {
                        floatingPoint = true;
                        if (!_source.Skip('+'))
                            _source.Skip('-');
                        if (!_source.Skip(x => IsDigit(x) || x== '_'))
                            throw new CodeException("syntax error: invalid numeric literal", TokenPosition);
                    }
                    number = _source.Extract(pos).Replace("_", "");

                    bool single = false;
                    if (_source.Current == 'f' || _source.Current == 'F')
                    {
                        _source.Next();
                        floatingPoint = true;
                        single = true;
                    }
                    else if (Source.Current == 'd' | _source.Current == 'D')
                    {
                        _source.Next();
                        floatingPoint = true;
                        single = false;
                    }

                    if (floatingPoint)
                    {
                        if (single)
                            value = Convert.ToSingle(number);
                        else
                            value = Convert.ToDouble(number);
                    }
                    else
                        value = Convert.ToUInt64(number, 10);
                }
            }
            catch (Exception)
            {
                throw new CodeException("syntax error: invalid numeric literal", TokenPosition);
            }


            if (!floatingPoint)
            {
                var uvalue = (ulong)value;

                System.Type targetType = null;

                if (_source.SkipI("IB"))
                {
                    targetType = typeof(sbyte);
                }
                else if (_source.SkipI("UB"))
                {
                    targetType = typeof(byte);
                }
                else if (_source.SkipI("IS"))
                {
                    targetType = typeof(short);
                }
                else if (_source.SkipI("US"))
                {
                    targetType = typeof(ushort);
                }
                else if (_source.SkipI("UL") || _source.Skip("LU"))
                {
                    targetType = typeof(ulong);
                }
                else if (_source.SkipI("U"))
                {
                    if ((uvalue & 0xFFFFFFFF00000000UL) != 0)
                    {
                        targetType = typeof(ulong);
                    }
                    else
                    {
                        targetType = typeof(uint);
                    }
                }
                else if (_source.SkipI("L"))
                {
                    if ((uvalue & 0x8000_0000_0000_0000UL) != 0)
                    {
                        targetType = typeof(ulong);
                    }
                    else
                    {
                        targetType = typeof(long);
                    }
                }
                else
                {
                    if ((uvalue & 0xFFFFFFFF00000000UL) != 0)
                    {
                        if ((uvalue & 0x8000_0000_0000_0000UL) != 0)
                        {
                            targetType = typeof(ulong);
                        }
                        else
                        {
                            targetType = typeof(long);
                        }
                    }
                    else
                    {
                        if ((uvalue & 0x0000_0000_8000_0000UL) != 0)
                        {
                            targetType = typeof(uint);
                        }
                        else
                        {
                            targetType = typeof(int);
                        }
                    }
                }

                if (targetType != null)
                {
                    try
                    {
                        value = Convert.ChangeType(value, targetType);
                    }
                    catch
                    {
                        throw new CodeException($"syntax error: can't convert numeric literal to {targetType}", TokenPosition);
                    }
                }
            }


            if (IsIdentifierChar(_source.Current))
            {
                throw new CodeException("syntax error: invalid suffix on numeric literal", TokenPosition);
            }

            return value;
        }

        bool IsDigit(char ch)
        {
            return ch >= '0' && ch <= '9';
        }

        bool IsHexDigit(char ch)
        {
            return (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F') || (ch >= '0' && ch <= '9');
        }

        bool IsBinaryDigit(char ch)
        {
            return (ch >= '0' && ch <= '1');
        }

        bool IsOctalDigit(char ch)
        {
            return (ch >= '0' && ch <= '7');
        }

        bool IsIdentifierLeadChar(char ch)
        {
            return (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || ch == '_' || ch == '$';
        }

        bool IsIdentifierChar(char ch)
        {
            return (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <='9') || ch == '_' || ch == '$';
        }
    }
}
