using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GDDL.Util
{
    public static class Utility
    {
        /**
         * Calculates the next power of two bigger than the given number
         * @param n The number to calculate the magnitude of
         * @return The power of two number
         */
        public static int UpperPower(int x)
        {
            // Ooooh... I just got how this works! Clever!
            // It's causing all the bits to spread downward
            // until all the bits below the most-significant 1
            // are also 1, then adds 1 to fill the power of two.

            x--;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            return x + 1;
        }

        /**
         * Validates if the given string contains a sequence of characters that is a valid identifier in GDDL.
         * @param text The string to validate
         * @return True if the string is a valid identifier
         */
        public static bool IsValidIdentifier(string text)
        {
            bool first = true;

            foreach (char c in text)
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

        /**
         * Replaces any disallowed characters with escape codes, assuming a `"` delimiter.
         * @param text The string to escape
         * @return The escaped string
         */
        public static string EscapeString(string p)
        {
            return EscapeString(p, '"');
        }

        /**
         * Replaces any disallowed characters with escape codes, using the given delimiter as a disallowed character.
         * @param text The string to escape
         * @param delimiter The delimiter that will surround the string
         * @return The escaped string
         */
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
                        sb.Append($"u{(int)c:X4}");
                    else
                        sb.Append($"x{(int)c:X2}");
                    break;
                }
            }

            sb.Append(delimiter);

            return sb.ToString();
        }

        /**
         * Validates if a character is valid within a quoted string.
         * @param c The character
         * @param delimiter The delimiter used for the string
         * @return True if the character is valid
         */
        private static bool IsValidStringCharacter(char c, char delimiter)
        {
            return IsPrintable(c) && !IsControl(c) && c != delimiter && c != '\\';
        }

        /**
         * Processes any escape sequences in the string, replacing them with the codepoints those sequences represent.
         * @param text The text to unescape
         * @return The unescaped string
         */
        public static string UnescapeString(string text)
        {
            var sb = new StringBuilder();

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
                        if (c == '\\')
                        {
                            inEscape = true;
                        }
                        else
                        {
                            sb.Append(c);
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
            (1 << (int)UnicodeCategory.Format) |
            (1 << (int)UnicodeCategory.Surrogate);

        /**
         * Determines if a character is printable.
         * A printable character is a character that can be used for display.
         * Non-printable characters are line separators, paragraph separators, other control characters,
         *  codepoints representing the (unmatched) halves of a surrogate pair, and private use characters.
         * @param c The character
         * @return True if the character is deemed printable
         */
        public static bool IsPrintable(char c)
        {
            return ((NON_PRINTABLE >> (int)char.GetUnicodeCategory(c)) & 1) == 0;
        }

        /**
         * Determines if a character is a letter, as per the unicode rules.
         * See {@link Character#isLetter(char)}
         * @param c The character
         * @return True if the character is a letter
         */
        public static bool IsLetter(int c)
        {
            return char.IsLetter((char)c);
        }

        /**
         * Determines if a character is a numeric digit, as per the unicode rules.
         * See {@link Character#isDigit(char)}
         * @param c The character
         * @return True if the character is a digit
         */
        public static bool IsDigit(int c)
        {
            return char.IsDigit((char)c);
        }

        /**
         * Determines if a character is a control character, as per the unicode rules.
         * See {@link Character#isISOControl(char)}
         * @param c The character
         * @return True if the character is a control character
         */
        public static bool IsControl(int c)
        {
            return char.IsControl((char)c);
        }

        public static int CompareOrdinalIgnoreCase(this string a, string b)
        {
            return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
        }

        public static T RequireNotNull<T>(T obj)
            where T : class
        {
            if (obj is null)
                throw new NullReferenceException();
            return (T)obj;
        }

        public static bool ListEquals<T>(List<T> a, List<T> b)
        {
            if (a.Count != b.Count)
                return false;

            for (int i = 0; i < a.Count; i++)
            {
                if (!Equals(a[i], b[i]))
                    return false;
            }

            return true;
        }
    }
}