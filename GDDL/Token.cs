
namespace GDDL
{
    public class Token : IContextProvider
    {
        public readonly string Comment;
        public readonly Tokens Name;
        public readonly string Text;
        public readonly ParsingContext Context;

        public Token(string comment, Tokens name, IContextProvider context, string text)
        {
            Comment = comment;
            Name = name;
            Text = text;
            Context = context.GetParsingContext();
        }

        public override string ToString()
        {
            if (Text == null)
                return $"({Name} @ {Context.Line}:{Context.Column})";

            if (Text.Length > 22)
                return $"({Name} @ {Context.Line}:{Context.Column}: {Text.Substring(0, 20)}...)";

            return $"({Name} @ {Context.Line}:{Context.Column}: {Text})";
        }

        public ParsingContext GetParsingContext()
        {
            return Context;
        }
    }
}
