using System;

namespace GDDL.Parsing
{
    public class Token : IContextProvider, IEquatable<Token>
    {
        public readonly string Whitespace;
        public readonly string Comment;
        public readonly TokenType Type;
        public readonly string Text;
        public ParsingContext ParsingContext { get; }

        public Token Parent { get; set; }

        public Token(TokenType name, string text, IContextProvider context, string comment, string whitespace)
        {
            Comment = comment;
            Whitespace = whitespace;
            Type = name;
            Text = text;
            ParsingContext = context.ParsingContext;
        }

        public Token Specialize(TokenType child)
        {
            return new Token(child, Text, this, Comment, Whitespace) { Parent = this };
        }

        public override string ToString()
        {
            if (Text == null)
                return $"({Type} @ {ParsingContext.Line}:{ParsingContext.Column})";

            if (Text.Length > 22)
                return $"({Type} @ {ParsingContext.Line}:{ParsingContext.Column}: {Text[..20]}...)";

            return $"({Type} @ {ParsingContext.Line}:{ParsingContext.Column}: {Text})";
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other == null || GetType() != other.GetType()) return false;
            return EqualsImpl((Token)other);
        }

        public bool Equals(Token other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other == null) return false;
            return EqualsImpl(other);
        }

        private bool EqualsImpl(Token other)
        {
            return Type == other.Type &&
                   Text == other.Text &&
                   Equals(ParsingContext, other.ParsingContext) &&
                   ((string.IsNullOrEmpty(Comment) && string.IsNullOrEmpty(other.Comment)) ||
                    Equals(Comment, other.Comment));
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Text, ParsingContext, Comment);
        }

        public bool Is(TokenType tokenType)
        {
            return Type == tokenType || (Parent != null && Parent.Is(tokenType));
        }
    }
}