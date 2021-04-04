using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GDDL.Util
{
    public static class Utility
    {
        // Ooooh... I just got how this works! Clever!
        // It's causing all the bits to spread downward
        // until all the bits below the most-significant 1
        // are also 1, then adds 1 to fill the power of two.
        public static int UpperPower(int x)
        {
            x--;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            return x + 1;
        }

        public static bool IsValidIdentifier(string ident)
        {
            bool first = true;

            foreach (char c in ident)
            {
                if (!Utility.IsLetter(c) && c != '_')
                {
                    if (first || !Utility.IsDigit(c))
                    {
                        return false;
                    }
                }

                first = false;
            }

            return true;
        }

        public static string EscapeString(string p)
        {
            return EscapeString(p, '"');
        }

        public static string EscapeString(string p, char delimiter)
        {
            var sb = new StringBuilder();

            sb.Append(delimiter);
            foreach (char c in p)
            {
                if (IsValidStringCharacter(c, delimiter))
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
                        if (c > 0xFF)
                            sb.Append(string.Format("u%04x", (int)c));
                        else
                            sb.Append(string.Format("x%02x", (int)c));
                        break;
                }
            }
            sb.Append(delimiter);

            return sb.ToString();
        }

        private static bool IsValidStringCharacter(char c, char delimiter)
        {
            return Utility.IsPrintable(c) && !Utility.IsControl(c) && c != delimiter && c != '\\';
        }

        public static string UnescapeString(string text)
        {
            StringBuilder sb = new StringBuilder();

            char startQuote = (char)0;

            bool inEscape = false;

            bool inHexEscape = false;
            int escapeAcc = 0;
            int escapeDigits = 0;
            int escapeMax = 0;

            foreach (char c in text)
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
                        else if (Utility.IsDigit(c))
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

            throw new ArgumentException("Invalid string literal", nameof(text));
        }

        private const int NON_PRINTABLE =
                        (1 << (int)UnicodeCategory.LineSeparator) |
                        (1 << (int)UnicodeCategory.ParagraphSeparator) |
                        (1 << (int)UnicodeCategory.Control) |
                        (1 << (int)UnicodeCategory.PrivateUse) |
                        (1 << (int)UnicodeCategory.Surrogate);

        public static bool IsPrintable(char c)
        {
            return ((NON_PRINTABLE >> (int)char.GetUnicodeCategory(c)) & 1) == 0;
        }

        public static bool IsLetter(int c)
        {
            return char.IsLetter((char)c);
        }

        public static bool IsDigit(int c)
        {
            return char.IsDigit((char)c);
        }

        public static bool IsControl(int c)
        {
            return char.IsControl((char)c);
        }

        public static int CompareOrdinalIgnoreCase(this string a, string b)
        {
            return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
        }

        public static T RequireNotNull<T>(this T obj)
            where T: class
        {
            if (obj is null)
                throw new NullReferenceException();
            return (T)obj;
        }
    }
}
