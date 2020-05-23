using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GDDL.Config;

namespace GDDL.Structure
{
    public abstract class Element
    {
        public string Comment { get; internal set; }
        public string Name { get; internal set; }

        public virtual bool IsResolved => true;
        public virtual Element ResolvedValue => this;

        // Factory methods
        public static Collection Set(params Element[] initial)
        {
            return new Collection(initial);
        }

        public static Collection Set(IEnumerable<Element> initial)
        {
            return new Collection(initial);
        }

        public static Reference Backreference(bool rooted, params string[] parts)
        {
            return new Reference(rooted, parts);
        }

        public static Value NullValue()
        {
            return new Value();
        }

        public static Value BooleanValue(bool value)
        {
            return new Value(value);
        }

        public static Value IntValue(long num)
        {
            return new Value(num);
        }

        public static Value FloatValue(double num)
        {
            return new Value(num);
        }

        public static Value StringValue(string s)
        {
            return new Value(s ?? "");
        }

        // Actual instance methods
        public bool HasName()
        {
            return !string.IsNullOrEmpty(Name);
        }

        public bool HasComment() 
        {
            return !string.IsNullOrEmpty(Comment); 
        }

        public bool IsSet()
        {
            return this is Collection;
        }

        public Collection AsSet()
        {
            return (Collection)this;
        }

        // Set LINQ helpers
        public IList<Element> AsList() { return (Collection)this; }
        public IDictionary<string, Element> AsDictionary() { return (Collection)this; }

        public bool IsValue()
        {
            return this is Value;
        }

        public Value AsValue()
        {
            return (Value)this;
        }

        public virtual Element Simplify()
        {
            return this;
        }

        public virtual void Resolve(Element root, Element parent)
        {
        }

        protected abstract void ToStringImpl(StringBuilder builder, StringGenerationContext ctx);

        public sealed override string ToString()
        {
            return ToString(new StringGenerationContext(StringGenerationOptions.Compact));
        }

        public string ToString(StringGenerationContext ctx)
        {
            var builder = new StringBuilder();
            ToStringWithName(builder, ctx);
            return builder.ToString();
        }

        private static Regex CommentLineSplitter = new Regex("(?:(?:\n)|(?:\r\n))");

        internal void ToStringWithName(StringBuilder builder, StringGenerationContext ctx)
        {
            if (HasComment() && ctx.Options.writeComments)
            {
                foreach (var s in Comment.Split())
                {
                    ctx.AppendIndent(builder);
                    builder.Append("#");
                    builder.Append(s);
                    builder.Append("\n");
                }
            }
            ctx.AppendIndent(builder);
            if (HasName())
            {
                string sname = Name;
                if (!Lexer.IsValidIdentifier(sname))
                    sname = Lexer.EscapeString(sname);
                builder.Append(sname);
                builder.Append(" = ");
            }

            ToStringImpl(builder, ctx);
        }

        public abstract Element Copy();
        protected virtual void CopyTo(Element other)
        {
            if (HasName())
                other.Name = Name;
        }

        public Element WithName(string name)
        {
            this.Name = name;
            return this;
        }
    }
}