using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDDL
{
    class Lexer
    {
        readonly Deque<Token> lookAhead = new Deque<Token>();
        readonly Stack<int> prefixStack = new Stack<int>();

        readonly Reader reader;

        bool seenEnd = false;

        int prefixPos = 0;
        Token prefix;

        public Tokens Prefix { get { return prefix.Name; } }

        public Lexer(Reader r)
        {
            reader = r;
        }

        public void BeginPrefixScan()
        {
            prefixStack.Push(prefixPos);
        }

        public void NextPrefix()
        {
            Require(prefixPos + 1);

            prefix = lookAhead[prefixPos++];
        }

        public void EndPrefixScan()
        {
            prefixPos = prefixStack.Pop();

            if (prefixPos > 0)
            {
                prefix = lookAhead[prefixPos - 1];
            }
            else
            {
                prefix = null;
            }
        }

        private void Require(int count)
        {
            int needed = count - lookAhead.Count;
            if (needed > 0)
            {
                ReadAhead(needed);
            }
        }

        public Tokens Peek()
        {
            Require(1);

            return lookAhead[0].Name;
        }

        public Token Pop()
        {
            Require(2);

            var t = lookAhead.RemoveFront();

            return t;
        }

        private void ReadAhead(int needed)
        {
            while (needed-- > 0)
            {
                var t = ParseOne();

                lookAhead.AddBack(t);
            }
        }

        private Token ParseOne()
        {
            if (seenEnd)
                return new Token(Tokens.END, reader.GetFileContext(), "");

            int ich = reader.Peek();
            while (true)
            {
                if (ich < 0) return new Token(Tokens.END, reader.GetFileContext(), "");

                switch (ich)
                {
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        reader.Drop(1);

                        ich = reader.Peek();
                        break;
                    case '#':
                        // comment, skip until \r or \n
                        do
                        {
                            reader.Drop(1);

                            ich = reader.Peek();
                        }
                        while (ich > 0 && ich != '\n' && ich != '\r');
                        break;
                    default:
                        goto blah;
                }
            }

            blah:
            switch (ich)
            {
                case '{': return new Token(Tokens.LBRACE, reader.GetFileContext(), reader.Read(1));
                case '}': return new Token(Tokens.RBRACE, reader.GetFileContext(), reader.Read(1));
                case ',': return new Token(Tokens.COMMA, reader.GetFileContext(), reader.Read(1));
                case ':': return new Token(Tokens.COLON, reader.GetFileContext(), reader.Read(1));
                case '=': return new Token(Tokens.EQUALS, reader.GetFileContext(), reader.Read(1));
            }

            if (char.IsLetter((char)ich) || ich == '_')
            {
                int number = 1;
                while (true)
                {
                    ich = reader.Peek(number);
                    if (ich < 0)
                        break;

                    if (char.IsLetter((char)ich) || char.IsLetterOrDigit((char)ich) || ich == '_')
                    {
                        number++;
                    }
                    else
                    {
                        break;
                    }
                }

                var id = new Token(Tokens.IDENT, reader.GetFileContext(), reader.Read(number));

                if (string.Compare(id.Text, "nil", true) == 0) return new Token(Tokens.NIL, id.Context, id.Text);
                if (string.Compare(id.Text, "null", true) == 0) return new Token(Tokens.NULL, id.Context, id.Text);
                if (string.Compare(id.Text, "true", true) == 0) return new Token(Tokens.TRUE, id.Context, id.Text);
                if (string.Compare(id.Text, "false", true) == 0) return new Token(Tokens.FALSE, id.Context, id.Text);

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
                        number = count_escape_seq(number);
                    }
                    else
                    {
                        if (ich == '\r')
                        {
                            throw new LexerException(this, string.Format("Expected '\\r', found {0}", DebugChar(ich)));
                        }
                        number++;
                    }

                    ich = reader.Peek(number);
                }

                if (ich != startedWith)
                {
                    throw new LexerException(this, string.Format("Expected '{0}', found {1}", startedWith, DebugChar(ich)));
                }

                number++;

                return new Token(Tokens.STRING, reader.GetFileContext(), reader.Read(number));
            }

            if (char.IsDigit((char)ich) || ich == '.')
            {
                // numbers
                int number = 0;
                bool fractional = false;

                if (char.IsDigit((char)ich))
                {
                    if (reader.Peek(0) == '0' && reader.Peek(1) == 'x')
                    {
                        number = 2;

                        ich = reader.Peek(number);
                        while (char.IsNumber((char)ich) || (ich >= 'a' && ich <= 'f') || (ich >= 'A' && ich <= 'F'))
                        {
                            number++;

                            ich = reader.Peek(number);
                        }

                        return new Token(Tokens.HEXINT, reader.GetFileContext(), reader.Read(number));
                    }

                    number = 1;
                    ich = reader.Peek(number);
                    while (char.IsNumber((char)ich))
                    {
                        number++;

                        ich = reader.Peek(number);
                    }
                }

                if (ich == '.')
                {
                    fractional = true;

                    // skip the '.'
                    number++;

                    ich = reader.Peek(number);
                    if (!char.IsDigit((char)ich))
                        throw new LexerException(this, string.Format("Expected DIGIT, found {0}", (char)ich));

                    while (char.IsNumber((char)ich))
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
                        throw new LexerException(this, string.Format("Expected DIGIT, found {0}", (char)ich));

                    while (char.IsNumber((char)ich))
                    {
                        number++;

                        ich = reader.Peek(number);
                    }
                }

                if (fractional)
                    return new Token(Tokens.DOUBLE, reader.GetFileContext(), reader.Read(number));

                return new Token(Tokens.INTEGER, reader.GetFileContext(), reader.Read(number));
            }

            throw new LexerException(this, string.Format("Unexpected character: {0}", reader.Peek()));
        }

        private string DebugChar(int ich)
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
                    if(char.IsControl((char)ich))
                        return string.Format("'\\u{0:X4}'", ich);
                    return string.Format("'{0}'", (char)ich);
            }
        }

        private int count_escape_seq(int number)
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

                if (char.IsNumber((char)ich) || (ich >= 'a' && ich <= 'f') || (ich >= 'A' && ich <= 'F'))
                {
                    number++;

                    if (char.IsNumber((char)ich) || (ich >= 'a' && ich <= 'f') || (ich >= 'A' && ich <= 'F'))
                    {
                        number++;

                        if (char.IsNumber((char)ich) || (ich >= 'a' && ich <= 'f') || (ich >= 'A' && ich <= 'F'))
                        {
                            number++;

                            if (char.IsNumber((char)ich) || (ich >= 'a' && ich <= 'f') || (ich >= 'A' && ich <= 'F'))
                            {
                                number++;
                            }
                        }
                    }
                }
                return number;
            }

            throw new LexerException(this, string.Format("Unknown escape sequence \\{0}", ich));
        }

        public override string ToString()
        {
            return string.Format("{{Lexer ahead={0}, reader={1}}}", string.Join(", ", lookAhead), reader);
        }

        public ParseContext GetFileContext()
        {
            Require(1);
            return lookAhead[0].Context;
        }
    }
}
