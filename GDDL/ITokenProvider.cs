using System;
using System.Collections.Generic;
using System.Text;

namespace GDDL
{
    public interface ITokenProvider : IContextProvider, IDisposable
    {
        TokenType Peek();
        TokenType Peek(int index);

        Token Pop();

    }
}
