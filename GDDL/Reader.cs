using System.IO;
using System.Text;
using GDDL.Exceptions;
using GDDL.Util;

namespace GDDL
{
    public class Reader : IContextProvider
    {
        private readonly QueueList<int> unreadBuffer = new QueueList<int>();

        private readonly StreamReader dataSource;
        private readonly string sourceName;

        private bool endQueued;
        private int line = 1;
        private int column = 1;
        private int lastEol;

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
                    throw new ReaderException(this, "Tried to Read beyond the end of the file.");
                }

                int ch = dataSource.Read();
                unreadBuffer.Add(ch);
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

        public int Next()
        {
            int ch = unreadBuffer.Remove();

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
                int ch = Next();
                if (ch < 0)
                    throw new ReaderException(this, "Tried to Read beyond the end of the file.");
                b.Append((char)ch);
            }
            return b.ToString();
        }

        public void Skip(int count)
        {
            Require(count);
            while (count-- > 0)
                Next();
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            foreach (var ch in unreadBuffer)
            {
                b.Append((char)ch);
            }
            return $"{{Reader ahead={b}}}";
        }

        public ParsingContext GetParsingContext()
        {
            return new ParsingContext(sourceName, line, column);
        }
    }
}
