using System;
using System.Runtime.Serialization;

namespace GDDL
{
    [Serializable]
    internal class LexerErrorException : Exception
    {
        public LexerErrorException(Lexer context)
            : base(context.GetFileContext().ToString())
        {
        }

        public LexerErrorException(Lexer context, string message)
            : base(string.Format("{0}: {1}", context.GetFileContext(), message))
        {
        }
    }
}