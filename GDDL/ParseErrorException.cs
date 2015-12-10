using System;
using System.Runtime.Serialization;

namespace GDDL
{
    [Serializable]
    internal class ParseErrorException : Exception
    {
        public ParseErrorException(Parser context)
            : base(context.Lexer.GetFileContext().ToString())
        {
        }

        public ParseErrorException(Parser context, string message)
            : base(string.Format("{0}: {1}", context.Lexer.GetFileContext(), message))
        {
        }
    }
}