using System;
using System.Text;
using GDDL.Exceptions;
using GDDL.Util;

namespace GDDL.Parsing
{
    public sealed class Lexer : ITokenProvider
    {
        #region API

        public Lexer(Reader r)
        {
            reader = r;
        }

        public WhitespaceMode WhitespaceMode { get; set; }

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

        public Token PeekFull()
        {
            Require(1);

            return lookAhead[0];
        }

        public Token Pop()
        {
            Require(2);

            return lookAhead.Remove();
        }

        #endregion

        #region Implementation

        private readonly ArrayQueue<Token> lookAhead = new();
        private readonly StringBuilder whitespaceBuilder = new();
        private readonly StringBuilder commentBuilder = new();

        private readonly Reader reader;

        private bool seenEnd = false;


        private void Require(int count)
        {
            int needed = count - lookAhead.Count;
            if (needed > 0)
            {
                ReadAhead(needed);
            }
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
            var startContext = reader.ParsingContext;

            if (seenEnd)
                return MakeEndToken(startContext);

            WhitespaceAndComments();
            string comment = GetComment();
            string whitespace = GetWhitespace();

            int ich = reader.Peek();
            if (ich < 0) return MakeEndToken(startContext);

            switch (ich)
            {
            case '{': return new Token(TokenType.LBrace, reader.Read(1), startContext, comment, whitespace);
            case '}': return new Token(TokenType.RBrace, reader.Read(1), startContext, comment, whitespace);
            case '[': return new Token(TokenType.LBracket, reader.Read(1), startContext, comment, whitespace);
            case ']': return new Token(TokenType.RBracket, reader.Read(1), startContext, comment, whitespace);
            case ',': return new Token(TokenType.Comma, reader.Read(1), startContext, comment, whitespace);
            case ':': return new Token(TokenType.Colon, reader.Read(1), startContext, comment, whitespace);
            case '/': return new Token(TokenType.Slash, reader.Read(1), startContext, comment, whitespace);
            case '=': return new Token(TokenType.EqualSign, reader.Read(1), startContext, comment, whitespace);
            case '%': return new Token(TokenType.Percent, reader.Read(1), startContext, comment, whitespace);
            case '^': return new Token(TokenType.Caret, reader.Read(1), startContext, comment, whitespace);
            }

            if (Utility.IsLetter(ich) || ich == '_')
            {
                int number = 1;
                while (true)
                {
                    ich = reader.Peek(number);
                    if (ich < 0)
                        break;

                    if (Utility.IsLetter(ich) || Utility.IsDigit(ich) || ich == '_')
                    {
                        number++;
                    }
                    else
                    {
                        break;
                    }
                }

                var id = new Token(TokenType.Identifier, reader.Read(number), startContext, comment, whitespace);

                if (id.Text.CompareOrdinalIgnoreCase("nil") == 0) return id.Specialize(TokenType.Nil);
                if (id.Text.CompareOrdinalIgnoreCase("null") == 0) return id.Specialize(TokenType.Null);
                if (id.Text.CompareOrdinalIgnoreCase("true") == 0) return id.Specialize(TokenType.True);
                if (id.Text.CompareOrdinalIgnoreCase("false") == 0) return id.Specialize(TokenType.False);
                if (id.Text.CompareOrdinalIgnoreCase("boolean") == 0) return id.Specialize(TokenType.Boolean);
                if (id.Text.CompareOrdinalIgnoreCase("string") == 0) return id.Specialize(TokenType.String);
                if (id.Text.CompareOrdinalIgnoreCase("integer") == 0) return id.Specialize(TokenType.Integer);
                if (id.Text.CompareOrdinalIgnoreCase("decimal") == 0) return id.Specialize(TokenType.Decimal);

                return id;
            }

            if (ich == '"' || ich == '\'')
            {
                int startedWith = ich;
                int number = 1;

                ich = reader.Peek(number);
                while (ich != startedWith && ich >= 0)
                {
                    switch (ich)
                    {
                    case '\\':
                        number = CountEscapeSeq(number);
                        break;
                    case '\r':
                        number++;
                        ich = reader.Peek(number);
                        if (ich == '\n')
                        {
                            number++;
                        }

                        break;
                    default:
                        number++;
                        break;
                    }

                    ich = reader.Peek(number);
                }

                if (ich != startedWith)
                {
                    throw new LexerException(this, $"Expected '{startedWith}', found {DebugChar(ich)}");
                }

                number++;

                return new Token(TokenType.StringLiteral, reader.Read(number), startContext, comment, whitespace);
            }

            if (Utility.IsDigit(ich) || ich == '.' || ich == '+' || ich == '-')
            {
                // numbers
                int number = 0;
                bool fractional = false;

                if (ich == '.')
                {
                    ich = reader.Peek(1);
                    if (ich == '.')
                    {
                        ich = reader.Peek(2);
                        if (ich == '.')
                        {
                            return new Token(TokenType.TripleDot, reader.Read(3), startContext, comment, whitespace);
                        }

                        return new Token(TokenType.DoubleDot, reader.Read(2), startContext, comment, whitespace);
                    }

                    if (!Utility.IsDigit(ich) && (ich != 'I') && (ich != 'N'))
                    {
                        return new Token(TokenType.Dot, reader.Read(1), startContext, comment, whitespace);
                    }

                    ich = reader.Peek();
                }

                if (ich == '.' && reader.Peek(number + 1) == 'N' && reader.Peek(number + 2) == 'a' &&
                    reader.Peek(number + 3) == 'N')
                {
                    return new Token(TokenType.DecimalLiteral, reader.Read(number + 4), startContext, comment,
                        whitespace);
                }

                if (ich == '-' || ich == '+')
                {
                    number++;

                    ich = reader.Peek(number);
                }

                if (ich == '.' && reader.Peek(number + 1) == 'I' && reader.Peek(number + 2) == 'n' &&
                    reader.Peek(number + 3) == 'f')
                {
                    return new Token(TokenType.DecimalLiteral, reader.Read(number + 4), startContext, comment,
                        whitespace);
                }

                if (Utility.IsDigit(ich))
                {
                    if (reader.Peek(number) == '0' && reader.Peek(number + 1) == 'x')
                    {
                        number += 2;

                        ich = reader.Peek(number);
                        while (Utility.IsDigit(ich) || (ich >= 'a' && ich <= 'f') || (ich >= 'A' && ich <= 'F'))
                        {
                            number++;

                            ich = reader.Peek(number);
                        }

                        return new Token(TokenType.HexIntLiteral, reader.Read(number), startContext, comment,
                            whitespace);
                    }

                    number = 1;
                    ich = reader.Peek(number);
                    while (Utility.IsDigit(ich))
                    {
                        number++;

                        ich = reader.Peek(number);
                    }
                }

                bool doubleDot = false;
                if (ich == '.')
                {
                    if (reader.Peek(number + 1) == '.') // double-dot
                    {
                        doubleDot = true;
                    }
                    else
                    {
                        fractional = true;

                        // Skip the '.'
                        number++;

                        ich = reader.Peek(number);

                        while (Utility.IsDigit(ich))
                        {
                            number++;

                            ich = reader.Peek(number);
                        }
                    }
                }

                if (!doubleDot && (ich == 'e' || ich == 'E'))
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

                    if (!Utility.IsDigit(ich))
                        throw new LexerException(this, $"Expected DIGIT, found {(char)ich}");

                    while (Utility.IsDigit(ich))
                    {
                        number++;

                        ich = reader.Peek(number);
                    }
                }

                if (fractional)
                    return new Token(TokenType.DecimalLiteral, reader.Read(number), startContext, comment, whitespace);

                return new Token(TokenType.IntegerLiteral, reader.Read(number), startContext, comment, whitespace);
            }

            throw new LexerException(this, $"Unexpected character: {reader.Peek()}");
        }

        private void WhitespaceAndComments()
        {
            int ich = reader.Peek();

            whitespaceBuilder.Clear();
            commentBuilder.Clear();
            bool commentStarted = false;
            while (true)
            {
                if (ich < 0) break;

                switch (ich)
                {
                case ' ':
                case '\t':
                {
                    char cch = (char)ich;
                    whitespaceBuilder.Append(cch);
                    if (commentStarted)
                        commentBuilder.Append(cch);
                    reader.Skip(1);
                    ich = reader.Peek();
                    break;
                }
                case '\r':
                case '\n':
                {
                    char cch = (char)ich;
                    whitespaceBuilder.Append(cch);
                    if (commentStarted)
                        commentBuilder.Append(cch);
                    reader.Skip(1);
                    ich = reader.Peek();
                    if (cch == '\r' && ich == '\n')
                    {
                        cch = (char)ich;
                        whitespaceBuilder.Append(cch);
                        if (commentStarted)
                            commentBuilder.Append(cch);
                        reader.Skip(1);
                        ich = reader.Peek();
                    }

                    commentStarted = false;
                    break;
                }
                case '#':
                {
                    char cch = (char)ich;
                    whitespaceBuilder.Append(cch);
                    if (!commentStarted)
                    {
                        commentStarted = true;
                    }
                    else
                    {
                        commentBuilder.Append(cch);
                    }

                    reader.Skip(1);
                    ich = reader.Peek();
                    break;
                }
                default:
                {
                    if (!commentStarted)
                    {
                        return;
                    }
                    else
                    {
                        char cch = (char)ich;
                        whitespaceBuilder.Append(cch);
                        commentBuilder.Append(cch);
                        reader.Skip(1);
                        ich = reader.Peek();
                    }

                    break;
                }
                }
            }
        }

        private string GetComment()
        {
            return commentBuilder.Length > 0 ? commentBuilder.ToString() : "";
        }

        private string GetWhitespace()
        {
            return whitespaceBuilder.Length > 0 ? whitespaceBuilder.ToString() : "";
        }

        private Token MakeEndToken(ParsingContext startContext)
        {
            seenEnd = true;
            return new Token(TokenType.End, "", startContext, "", "");
        }

        private static string DebugChar(int ich)
        {
            if (ich < 0)
                return "EOF";

            return ich switch
            {
                0 => "'\\0'",
                8 => "'\\b'",
                9 => "'\\t'",
                10 => "'\\n'",
                13 => "'\\r'",
                _ => Utility.IsControl(ich) ? $"'\\u{ich:X4}'" : $"'{(char)ich}'",
            };
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
                if (Utility.IsDigit(ich) || (ich >= 'a' && ich <= 'f') || (ich >= 'A' && ich <= 'F'))
                {
                    number++;

                    ich = reader.Peek(number);
                    if (Utility.IsDigit(ich) || (ich >= 'a' && ich <= 'f') || (ich >= 'A' && ich <= 'F'))
                    {
                        number++;

                        ich = reader.Peek(number);
                        if (Utility.IsDigit(ich) || (ich >= 'a' && ich <= 'f') || (ich >= 'A' && ich <= 'F'))
                        {
                            number++;

                            ich = reader.Peek(number);
                            if (Utility.IsDigit(ich) || (ich >= 'a' && ich <= 'f') || (ich >= 'A' && ich <= 'F'))
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

        #endregion

        #region ToString

        public override string ToString()
        {
            return $"{{Lexer ahead={string.Join(", ", lookAhead)}, reader={reader}}}";
        }

        #endregion

        #region IContextProvider

        public ParsingContext ParsingContext
        {
            get
            {
                if (lookAhead.Count > 0)
                    return lookAhead[0].ParsingContext;
                return reader.ParsingContext;
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            reader.Dispose();
        }

        #endregion
    }
}