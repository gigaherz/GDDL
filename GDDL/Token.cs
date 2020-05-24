
using System;

namespace GDDL
{
    public class Token : IContextProvider, IEquatable<Token>
    {
        public readonly string Comment;
        public readonly TokenType Type;
        public readonly string Text;
        public readonly ParsingContext Context;

        public Token(TokenType name, string text, IContextProvider context, string comment)
        {
            Comment = comment;
            Type = name;
            Text = text;
            Context = context.GetParsingContext();
        }

        public override string ToString()
        {
            if (Text == null)
                return $"({Type} @ {Context.Line}:{Context.Column})";

            if (Text.Length > 22)
                return $"({Type} @ {Context.Line}:{Context.Column}: {Text.Substring(0, 20)}...)";

            return $"({Type} @ {Context.Line}:{Context.Column}: {Text})";
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if (obj == null || GetType() != obj.GetType()) return false;
            return obj is Token other ? EqualsImpl(other) : false;
        }

        public bool Equals(Token other)
        {
            if (other == this) return true;
            if (other == null) return false;
            return EqualsImpl(other);
        }

        private bool EqualsImpl(Token other)
        {
            return Type == other.Type &&
                Text == other.Text && 
                Equals(Context, other.Context) &&
                ((string.IsNullOrEmpty(Comment) && string.IsNullOrEmpty(other.Comment)) || Equals(Comment, other.Comment));
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Text, Context, Comment);
        }

        public ParsingContext GetParsingContext()
        {
            return Context;
        }
    }
}
