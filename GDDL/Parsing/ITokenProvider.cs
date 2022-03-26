using System;

namespace GDDL.Parsing
{
    public interface ITokenProvider : IContextProvider, IDisposable
    {
        /**
         * Returns the type of the first token in the lookahead buffer, reading new tokens from the Reader as necessary.
         * @return The token type.
         */
        TokenType Peek();

        /**
         * Returns the type of the first token in the lookahead buffer, reading new tokens from the Reader as necessary.
         * @return The token type.
         */
        Token PeekFull();

        /**
         * Returns the type of the Nth token in the lookahead buffer, reading new tokens from the Reader as necessary.
         * @param index The position from the lookahead buffer, starting at 0.
         * @return The token type at that position.
         */
        TokenType Peek(int index);

        /**
         * Removes the first token in the lookahead buffer, and returns it.
         * @return The token.
         */
        Token Pop();

        /**
         * Changes the whitespace processing mode.
         */
        WhitespaceMode WhitespaceMode { get; set; }
    }
}