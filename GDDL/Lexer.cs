using System;
using System.Text;
using GDDL.Exceptions;
using GDDL.Util;

namespace GDDL
{
    public sealed class Lexer : ITokenProvider
    {
        private readonly QueueList<Token> lookAhead = new QueueList<Token>();

        private readonly Reader reader;

        private bool seenEnd = false;

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
                        goto commentLoopExit;
                }
            }

        commentLoopExit:
            string comment = commentLines != null ? commentLines.ToString() : "";

            switch (ich)
            {
                case '{': return new Token(TokenType.LBrace, reader.Read(1), startContext, comment);
                case '}': return new Token(TokenType.RBrace, reader.Read(1), startContext, comment);
                case ',': return new Token(TokenType.Comma, reader.Read(1), startContext, comment);
                case ':': return new Token(TokenType.Colon, reader.Read(1), startContext, comment);
                case '=': return new Token(TokenType.EqualSign, reader.Read(1), startContext, comment);
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

            if (Utility.IsDigit(ich) || ich == '.' || ich == '+' || ich == '-')
            {
                // numbers
                int number = 0;
                bool fractional = false;

                if (ich == '.' && reader.Peek(number + 1) == 'N' && reader.Peek(number + 2) == 'a' && reader.Peek(number + 3) == 'N')
                {
                    return new Token(TokenType.Double, reader.Read(number + 4), startContext, comment);
                }

                if (ich == '-' || ich == '+')
                {
                    number++;

                    ich = reader.Peek(number);
                }

                if (ich == '.' && reader.Peek(number + 1) == 'I' && reader.Peek(number + 2) == 'n' && reader.Peek(number + 3) == 'f')
                {
                    return new Token(TokenType.Double, reader.Read(number + 4), startContext, comment);
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

                        return new Token(TokenType.HexInt, reader.Read(number), startContext, comment);
                    }

                    number = 1;
                    ich = reader.Peek(number);
                    while (Utility.IsDigit(ich))
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

                    while (Utility.IsDigit(ich))
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

                    if (!Utility.IsDigit(ich))
                        throw new LexerException(this, $"Expected DIGIT, found {(char)ich}");

                    while (Utility.IsDigit(ich))
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
                    return Utility.IsControl(ich) ? $"'\\u{ich:X4}'" : $"'{(char)ich}'";
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

        public override string ToString()
        {
            return $"{{Lexer ahead={string.Join(", ", lookAhead)}, reader={reader}}}";
        }

        public static string UnescapeString(Token t)
        {
            try
            {
                return Utility.UnescapeString(t.Text);
            }
            catch (ArgumentException e)
            {
                throw new ParserException(t, "Unescaping string", e);
            }
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
