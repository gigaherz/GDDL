using System;

namespace GDDL
{
    [Serializable]
    internal class ParserException : Exception
    {
        public ParserException(Parser context, string message)
            : base(string.Format("{0}: {1}", context.Lexer.GetFileContext(), message))
        {
        }
    }
}