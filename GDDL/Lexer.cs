using System;
using System.Text;
using GDDL.Exceptions;
using GDDL.Util;

namespace GDDL
{
    public sealed class Lexer : ITokenProvider
    {
        readonly QueueList<Token> lookAhead = new QueueList<Token>();

        readonly Reader reader;

        bool seenEnd = false;

        public Lexer(Reader r)
        {
            reader = r;
        }

        private void Require(int count)
        {
            int needed = count - lookAhead.Count;
            if (needed > 0)
            {
                ReadAhead(needed);
            }
        }

        public TokenType Peek(int pos)
        {
            Require(pos + 1);

            return lookAhead[pos].Type;
        }

        public TokenType Peek()
        {
            Require(1);

            return lookAhead[0].Type;
        }

        public Token Pop()
        {
            Require(2);

            return lookAhead.Remove();
        }

        private void ReadAhead(int needed)
        {
            while (needed-- > 0)
            {
                lookAhead.Add(ParseOne());
            }
        }

        private Token ParseOne()
        {
            var startContext = reader.GetParsingContext();

            if (seenEnd)
                return new Token(TokenType.End, "", startContext, "");

            StringBuilder commentLines = null;
            int ich = reader.Peek();

            while (true)
            {
                if (ich < 0) return new Token(TokenType.End, "", startContext, "");

                switch (ich)
                {
                    case ' ':
                    case '\t':
                        reader.Skip(1);
                        ich = reader.Peek();
                        break;
                    case '\r':
                    case '\n':
                        if (commentLines != null)
                        {
                            commentLines.Append(reader.Read(1));
                        }
                        else
                        {
                            reader.Skip(1);
                        }
                        ich = reader.Peek();
                        break;
                    case '#':
                    {
                        // comment
                        if (commentLines == null)
                        {
                            commentLines = new StringBuilder();
                        }

                        reader.Skip(1);
                        ich = reader.Peek();

                        int number = 0;
                        while (ich > 0 && ich != '\n' && ich != '\r')
                        {
                            number++;
                            ich = reader.Peek(number);
                        }

                        if (number > 0)
                        {
                            commentLines.Append(reader.Read(number));
                        }
                        ich = reader.Peek();

                        break;
                    }
                    default:
                        goto blah;
                }
            }

        blah:
            string comment = commentLines != null ? commentLines.ToString() : "";

            switch (ich)
            {
                case '{': return new Token(TokenType.LBrace, reader.Read(1), startContext, comment);
                case '}': return new Token(TokenType.RBrace, reader.Read(1), startContext, comment);
                case ',': return new Token(TokenType.Comma, reader.Read(1), startContext, comment);
                case ':': return new Token(TokenType.Colon, reader.Read(1), startContext, comment);
                case '=': return new Token(TokenType.EqualSign, reader.Read(1), startContext, comment);
            }

            if (char.IsLetter((char)ich) || ich == '_')
            {
                int number = 1;
                while (true)
                {
                    ich = reader.Peek(number);
                    if (ich < 0)
                        break;

                    if (char.IsLetter((char)ich) || char.IsDigit((char)ich) || ich == '_')
                    {
                        number++;
                    }
                    else
                    {
                        break;
                    }
                }

                var id = new Token(TokenType.Ident, reader.Read(number), startContext, comment);

                if (id.Text.CompareOrdinalIgnoreCase("nil") == 0) return new Token(TokenType.Nil, id.Text, id, comment);
                if (id.Text.CompareOrdinalIgnoreCase("null") == 0) return new Token(TokenType.Null, id.Text, id, comment);
                if (id.Text.CompareOrdinalIgnoreCase("true") == 0) return new Token(TokenType.True, id.Text, id, comment);
                if (id.Text.CompareOrdinalIgnoreCase("false") == 0) return new Token(TokenType.False, id.Text, id, comment);

                return id;
            }

            if (ich == '"' || ich == '\'')
            {
                int startedWith = ich;
                int number = 1;

                ich = reader.Peek(number);
                while (ich != startedWith && ich >= 0)
                {
                    if (ich == '\\')
                    {
                        number = CountEscapeSeq(number);
                    }
                    else
                    {
                        if (ich == '\r')
                        {
                            throw new LexerException(this, $"Expected '\\r', found {DebugChar(ich)}");
                        }
                        number++;
                    }

                    ich = reader.Peek(number);
                }

                if (ich != startedWith)
                {
                    throw new LexerException(this, $"Expected '{startedWith}', found {DebugChar(ich)}");
                }

                number++;

                return new Token(TokenType.String, reader.Read(number), startContext, comment);
            }

            if (char.IsDigit((char)ich) || ich == '-' || ich == '.')
            {
                // numbers
                int number = 0;
                bool fractional = false;

                if (ich == '-')
                {
                    number++;

                    ich = reader.Peek(number);
                }

                if (char.IsDigit((char)ich))
                {
                    if (reader.Peek(number) == '0' && reader.Peek(number + 1) == 'x')
                    {
                        number += 2;

                        ich = reader.Peek(number);
                        while (char.IsDigit((char)ich) || (ich >= 'a' && ich <= 'f') || (ich >= 'A' && ich <= 'F'))
                        {
                            number++;

                            ich = reader.Peek(number);
                        }

                        return new Token(TokenType.HexInt, reader.Read(number), startContext, comment);
                    }

                    number = 1;
                    ich = reader.Peek(number);
                    while (char.IsDigit((char)ich))
                    {
                        number++;

                        ich = reader.Peek(number);
                    }
                }

                if (ich == '.')
                {
                    fractional = true;

                    // Skip the '.'
                    number++;

                    ich = reader.Peek(number);

                    while (char.IsDigit((char)ich))
                    {
                        number++;

                        ich = reader.Peek(number);
                    }
                }

                if (ich == 'e' || ich == 'E')
                {
                    fractional = true;

                    // letter
                    number++;

                    ich = reader.Peek(number);
                    if (ich == '+' || ich == '-')
                    {
                        number++;

                        ich = reader.Peek(number);
                    }

                    if (!char.IsDigit((char)ich))
                        throw new LexerException(this, $"Expected DIGIT, found {(char)ich}");

                    while (char.IsDigit((char)ich))
                    {
                        number++;

                        ich = reader.Peek(number);
                    }
                }

                if (fractional)
                    return new Token(TokenType.Double, reader.Read(number), startContext, comment);

                return new Token(TokenType.Integer, reader.Read(number), startContext, comment);
            }

            throw new LexerException(this, $"Unexpected character: {reader.Peek()}");
        }

        private static string DebugChar(int ich)
        {
            if (ich < 0)
                return "EOF";

            switch (ich)
            {
                case 0: return "'\\0'";
                case 8: return "'\\b'";
                case 9: return "'\\t'";
                case 10: return "'\\n'";
                case 13: return "'\\r'";
                default:
                    return char.IsControl((char)ich) ? $"'\\u{ich:X4}'" : $"'{(char)ich}'";
            }
        }

        private int CountEscapeSeq(int number)
        {
            int ich = reader.Peek(number);
            if (ich != '\\')
                throw new LexerException(this, "Internal Error");

            number++;

            ich = reader.Peek(number);
            switch (ich)
            {
                case '0':
                case 'b':
                case 'f':
                case 'n':
                case 'r':
                case 't':
                case '"':
                case '\'':
                case '\\':
                    return ++number;
            }

            if (ich == 'x' || ich == 'u')
            {
                number++;

                ich = reader.Peek(number);
                if (char.IsDigit((char)ich) || (ich >= 'a' && ich <= 'f') || (ich >= 'A' && ich <= 'F'))
                {
                    number++;

                    ich = reader.Peek(number);
                    if (char.IsDigit((char)ich) || (ich >= 'a' && ich <= 'f') || (ich >= 'A' && ich <= 'F'))
                    {
                        number++;

                        ich = reader.Peek(number);
                        if (char.IsDigit((char)ich) || (ich >= 'a' && ich <= 'f') || (ich >= 'A' && ich <= 'F'))
                        {
                            number++;

                            ich = reader.Peek(number);
                            if (char.IsDigit((char)ich) || (ich >= 'a' && ich <= 'f') || (ich >= 'A' && ich <= 'F'))
                            {
                                number++;
                            }
                        }
                    }
                }
                return number;
            }

            throw new LexerException(this, $"Unknown escape sequence \\{ich}");
        }

        public override string ToString()
        {
            return $"{{Lexer ahead={string.Join(", ", lookAhead)}, reader={reader}}}";
        }

        public static bool IsValidIdentifier(string ident)
        {
            bool first = true;

            foreach (char c in ident)
            {
                if (!char.IsLetter(c) && c != '_')
                {
                    if (first || !char.IsDigit(c))
                    {
                        return false;
                    }
                }

                first = false;
            }

            return true;
        }

        public static string UnescapeString(Token t)
        {
            StringBuilder sb = new StringBuilder();

            char startQuote = (char)0;

            bool inEscape = false;

            bool inHexEscape = false;
            int escapeAcc = 0;
            int escapeDigits = 0;
            int escapeMax = 0;

            foreach (char c in t.Text)
            {
                if (startQuote != 0)
                {
                    if (inHexEscape)
                    {
                        if (escapeDigits == escapeMax)
                        {
                            sb.Append((char)escapeAcc);
                            inHexEscape = false;
                        }
                        else if (char.IsDigit(c))
                        {
                            escapeAcc = (escapeAcc << 4) + (c - '0');
                        }
                        else if ((escapeDigits < escapeMax) && (c >= 'a') && (c <= 'f'))
                        {
                            escapeAcc = (escapeAcc << 4) + 10 + (c - 'a');
                        }
                        else if ((escapeDigits < escapeMax) && (c >= 'A') && (c <= 'F'))
                        {
                            escapeAcc = (escapeAcc << 4) + 10 + (c - 'A');
                        }
                        else
                        {
                            sb.Append((char)escapeAcc);
                            inHexEscape = false;
                        }
                        escapeDigits++;
                    }

                    if (inEscape)
                    {
                        switch (c)
                        {
                            case '"':
                                sb.Append('"');
                                break;
                            case '\'':
                                sb.Append('\'');
                                break;
                            case '\\':
                                sb.Append('\\');
                                break;
                            case '0':
                                sb.Append('\0');
                                break;
                            case 'b':
                                sb.Append('\b');
                                break;
                            case 't':
                                sb.Append('\t');
                                break;
                            case 'n':
                                sb.Append('\n');
                                break;
                            case 'f':
                                sb.Append('\f');
                                break;
                            case 'r':
                                sb.Append('\r');
                                break;
                            case 'x':
                                inHexEscape = true;
                                escapeAcc = 0;
                                escapeDigits = 0;
                                escapeMax = 2;
                                break;
                            case 'u':
                                inHexEscape = true;
                                escapeAcc = 0;
                                escapeDigits = 0;
                                escapeMax = 4;
                                break;
                        }
                        inEscape = false;
                    }
                    else if (!inHexEscape)
                    {
                        if (c == startQuote)
                            return sb.ToString();
                        switch (c)
                        {
                            case '\\':
                                inEscape = true;
                                break;
                            default:
                                sb.Append(c);
                                break;
                        }
                    }
                }
                else
                {
                    switch (c)
                    {
                        case '"':
                            startQuote = '"';
                            break;
                        case '\'':
                            startQuote = '\'';
                            break;
                        default:
                            sb.Append(c);
                            break;
                    }
                }
            }

            throw new ParserException(t, "Invalid string literal");
        }

        public static string EscapeString(string p)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append('"');
            foreach (char c in p)
            {
                bool printable = (c >= 32 && c < 127)
                                 || char.IsWhiteSpace(c)
                                 || char.IsLetter(c)
                                 || char.IsDigit(c);
                if (!char.IsControl(c) && printable && c != '"' && c != '\\')
                {
                    sb.Append(c);
                    continue;
                }

                sb.Append('\\');
                switch (c)
                {
                    case '\b':
                        sb.Append('b');
                        break;
                    case '\t':
                        sb.Append('t');
                        break;
                    case '\n':
                        sb.Append('n');
                        break;
                    case '\f':
                        sb.Append('f');
                        break;
                    case '\r':
                        sb.Append('r');
                        break;
                    case '\"':
                        sb.Append('\"');
                        break;
                    case '\\':
                        sb.Append('\\');
                        break;
                    default:
                        sb.Append(c > 0xFF ? $"u{(int)c:X4}" : $"u{(int)c:X2}");
                        break;
                }
            }
            sb.Append('"');

            return sb.ToString();
        }

        public ParsingContext GetParsingContext()
        {
            if (lookAhead.Count > 0)
                return lookAhead[0].Context;
            return reader.GetParsingContext();
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
