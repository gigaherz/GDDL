using System;
using System.IO;
using System.Text;
using GDDL.Exceptions;
using GDDL.Util;

namespace GDDL
{
    public sealed class Reader : IContextProvider, IDisposable
    {
        private readonly ArrayQueue<int> unreadBuffer = new ArrayQueue<int>();

        private readonly TextReader dataSource;
        private readonly string sourceName;

        private bool endQueued;
        private int line = 1;
        private int column = 1;
        private int lastEol;

        public Reader(TextReader source, string sourceName)
        {
            this.sourceName = sourceName;
            this.dataSource = source;
        }

        private void Require(int number)
        {
            int needed = number - unreadBuffer.Count;
            if (needed > 0)
            {
                NeedChars(needed);
            }
        }

        private void NeedChars(int needed)
        {
            while (needed-- > 0)
            {
                if (endQueued)
                {
                    throw new ReaderException(this, "Tried to Read beyond the end of the file.");
                }

                int ch = dataSource.Read();
                unreadBuffer.Add(ch);
                if (ch < 0)
                    endQueued = true;
            }
        }

        /**
         * Returns the first character in the lookahead buffer, reading characters from the input reader as needed.
         * @return The character, or -1 if end of file
         */
        public int Peek()
        {
            return Peek(0);
        }

        /**
         * Returns the Nth character in the lookahead buffer, reading characters from the input reader as needed.
         * @param index The position in the lookahead buffer, starting at 0.
         * @return The character, or -1 if end of file
         */
        public int Peek(int index)
        {
            Require(index + 1);

            return unreadBuffer[index];
        }

        /**
         * Removes the first character in the lookahead buffer, and returns it.
         * @return The character, or -1 if end of file
         */
        public int Next()
        {
            int ch = unreadBuffer.Remove();

            column++;
            if (ch == '\n')
            {
                column = 1;
                if (lastEol != '\r')
                    line++;
                lastEol = ch;
            }
            else if (ch == '\r')
            {
                column = 1;
                line++;
                lastEol = ch;
            }
            else if (lastEol > 0)
            {
                lastEol = 0;
            }

            return ch;
        }

        /**
         * Removes N characters from the lookahead buffer, and returns them as a string.
         * @param count The number of characters to return
         * @return A string with the character sequence
         */
        public string Read(int count)
        {
            Require(count);
            var b = new StringBuilder();
            while (count-- > 0)
            {
                int ch = Next();
                if (ch < 0)
                    throw new ReaderException(this, "Tried to Read beyond the end of the file.");
                b.Append((char)ch);
            }
            return b.ToString();
        }

        /**
         * Removes N characters from the lookahead buffer, advancing the input stream as necessary.
         * @param count The number of characters to drop
         */
        public void Skip(int count)
        {
            Require(count);
            while (count-- > 0)
                Next();
        }

        public override string ToString()
        {
            return $"{{Reader ahead={string.Join("", unreadBuffer)}}}";
        }

        public void Dispose()
        {
            dataSource.Dispose();
        }

        public ParsingContext ParsingContext => new ParsingContext(sourceName, line, column);
    }
}
