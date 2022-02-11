using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GDDL.Parsing;

namespace GDDL
{
    public static class GDDL
    {
        /**
         * Constructs a Parser instance that reads from the given filename.
         * @param filename The filename to read from.
         * @return A parser ready to process the file.
         */
        public static Parser FromFile(string filename)
        {
            return FromFile(filename, Encoding.UTF8);
        }

        /**
         * Constructs a Parser instance that reads from the given filename.
         * @param filename The filename to read from.
         * @param charset The charset.
         * @return A parser ready to process the file.
         */
        public static Parser FromFile(string filename, Encoding encoding)
        {
            return FromReader(new StreamReader(filename, encoding), filename);
        }

        /**
         * Constructs a Parser instance that reads from the given file.
         * @param file The file to read from.
         * @return A parser ready to process the file.
         */
        public static Parser FromFile(FileInfo file)
        {
            return FromFile(file.FullName);
        }

        /**
         * Constructs a Parser instance that reads from the given file.
         * @param file The file to read from.
         * @param charset The charset.
         * @return A parser ready to process the file.
         */
        public static Parser FromFile(FileInfo file, Encoding encoding)
        {
            return FromFile(file.FullName, encoding);
        }

        /**
         * Constructs a Parser instance that reads from the given string.
         * @param text The text to parse.
         * @return A parser ready to process the file.
         */
        public static Parser FromString(string text, string sourceName = "UNKNOWN")
        {
            return FromReader(new StringReader(text), sourceName);
        }

        /**
         * Constructs a Parser instance that reads from the given reader.
         * @param reader The stream to read from.
         * @return A parser ready to process the file.
         */
        public static Parser FromReader(TextReader text, string sourceName = "UNKNOWN")
        {
            return new Parser(new Lexer(new Reader(text, sourceName)));
        }

        // For unit test purposes
        public static Parser FromProvider(ITokenProvider lexer)
        {
            return new Parser(lexer);
        }

    }
}
