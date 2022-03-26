using System;
using System.IO;
using System.Text;
using GDDL.Exceptions;
using GDDL.Util;

namespace GDDL.Parsing
{
    public sealed class Reader : IContextProvider, IDisposable
    {
        #region API

        public Reader(TextReader source, string sourceName)
        {
            this.sourceName = sourceName;
            this.dataSource = source;
        }

        /**
         * Returns the Nth character in the lookahead buffer, reading characters from the input reader as needed.
         * @param index The position in the lookahead buffer, starting at 0.
         * @return The character, or -1 if end of file
         */
        public int Peek(int index = 0)
        {
            Require(index + 1);

            return unreadBuffer[index];
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
                int ch = NextInternal();
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
                NextInternal();
        }

        #endregion

        #region Implementation

        private readonly ArrayQueue<int> unreadBuffer = new();

        private readonly TextReader dataSource;
        private readonly string sourceName;

        private bool endQueued;
        private int line = 1;
        private int column = 1;
        private int lastEol;

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

        private int NextInternal()
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

        #endregion

        #region ToString

        public override string ToString()
        {
            return $"{{Reader ahead={string.Join("", unreadBuffer)}}}";
        }

        #endregion

        #region IContextProvider

        public ParsingContext ParsingContext => new(sourceName, line, column);

        #endregion

        #region IDisposable

        public void Dispose()
        {
            dataSource.Dispose();
        }

        #endregion
    }
}