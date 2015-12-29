using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDDL
{
    class Reader
    {
        bool endQueued = false;
        readonly Deque<int> unreadBuffer = new Deque<int>();

        TextReader dataSource;
        string sourceName;
        int line = 1;
        int column = 1;

        int lastEol;

        public Reader(string source)
        {
            sourceName = source;
            dataSource = new StreamReader(source);
        }

        void Require(int number)
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
                    throw new ReaderException(this, "Tried to read beyond the end of the file.");
                }

                int ch = dataSource.Read();
                unreadBuffer.AddBack(ch);
                if (ch < 0)
                    endQueued = true;
            }
        }

        public int Peek()
        {
            return Peek(0);
        }

        public int Peek(int index)
        {
            Require(index + 1);

            return unreadBuffer[index];
        }

        public int Pop()
        {
            int ch = unreadBuffer.RemoveFront();

            column++;
            if (ch == '\n')
            {
                if (lastEol != '\r')
                {
                    column = 1;
                    line++;
                }
                lastEol = ch;
            }
            else if (ch == '\r')
            {
                lastEol = ch;
            }
            else if (lastEol > 0)
            {
                lastEol = 0;
                column = 1;
                line++;
            }

            return ch;
        }

        public string Read(int count)
        {
            Require(count);
            StringBuilder b = new StringBuilder();
            while (count-- > 0)
            {
                var ch = Pop();
                if (ch < 0)
                    throw new ReaderException(this, "Tried to read beyond the end of the file.");
                b.Append((char)ch);
            }
            return b.ToString();
        }

        public void Drop(int count)
        {
            Require(count);
            while (count-- > 0)
                Pop();
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            foreach (var ch in unreadBuffer)
            {
                b.Append((char)ch);
            }
            return string.Format("{{Reader ahead={0}}}", b.ToString());
        }

        internal ParseContext GetFileContext()
        {
            return new ParseContext(sourceName, line, column);
        }
    }
}
