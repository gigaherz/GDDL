using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GDDL.Parsing;
using GDDL.Structure;

namespace GDDL
{
    public static class Gddl
    {
        /**
         * Constructs a Parser instance that reads from the given filename.
         * @param filename The filename to read from.
         * @return A parser ready to process the file.
         */
        public static GddlDocument FromFile(string filename)
        {
            return FromFile(filename, Encoding.UTF8);
        }

        /**
         * Constructs a Parser instance that reads from the given filename.
         * @param filename The filename to read from.
         * @param charset The charset.
         * @return A parser ready to process the file.
         */
        public static GddlDocument FromFile(string filename, Encoding encoding)
        {
            return FromReader(new StreamReader(filename, encoding), filename);
        }

        /**
         * Constructs a Parser instance that reads from the given file.
         * @param file The file to read from.
         * @return A parser ready to process the file.
         */
        public static GddlDocument FromFile(FileInfo file)
        {
            return FromFile(file.FullName);
        }

        /**
         * Constructs a Parser instance that reads from the given file.
         * @param file The file to read from.
         * @param charset The charset.
         * @return A parser ready to process the file.
         */
        public static GddlDocument FromFile(FileInfo file, Encoding encoding)
        {
            return FromFile(file.FullName, encoding);
        }

        /**
         * Constructs a Parser instance that reads from the given string.
         * @param text The text to parse.
         * @return A parser ready to process the file.
         */
        public static GddlDocument FromString(string text, string sourceName = "UNKNOWN")
        {
            return FromReader(new StringReader(text), sourceName);
        }

        /**
         * Constructs a Parser instance that reads from the given reader.
         * @param reader The stream to read from.
         * @return A parser ready to process the file.
         */
        public static GddlDocument FromReader(TextReader text, string sourceName = "UNKNOWN")
        {
            var parser = new Parser(new Lexer(new Reader(text, sourceName)));
            return parser.Parse();
        }

        // For unit test purposes
        public static GddlDocument FromProvider(ITokenProvider lexer)
        {
            var parser = new Parser(lexer);
            return parser.Parse();
        }

    }
}
