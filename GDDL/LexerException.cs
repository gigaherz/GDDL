using System;
using System.Runtime.Serialization;

namespace GDDL
{
    [Serializable]
    internal class LexerException : Exception
    {
        public LexerException(Lexer context, string message)
            : base(string.Format("{0}: {1}", context.GetFileContext(), message))
        {
        }
    }
}