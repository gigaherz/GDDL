using System.Collections.Generic;
using GDDL.Config;

namespace GDDL.Structure
{
    public abstract class Element
    {
        public string Name { get; internal set; }

        public virtual bool IsResolved => true;
        public virtual Element ResolvedValue => this;

        // Factory methods
        public static Set Set(params Element[] initial)
        {
            return new Set(initial);
        }

        public static Set Set(IEnumerable<Element> initial)
        {
            return new Set(initial);
        }

        public static Backreference Backreference(bool rooted, params string[] parts)
        {
            return new Backreference(rooted, parts);
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
            return new Value(s);
        }

        // Actual instance methods
        public bool HasName()
        {
            return !string.IsNullOrEmpty(Name);
        }

        public bool IsSet()
        {
            return this is Set;
        }

        public Set AsSet()
        {
            return (Set)this;
        }

        // Set LINQ helpers
        public IList<Element> AsList() { return (Set)this; }
        public IDictionary<string, Element> AsDictionary() { return (Set)this; }

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

        protected abstract string ToStringInternal(StringGenerationContext ctx);

        public sealed override string ToString()
        {
            return ToString(new StringGenerationContext(StringGenerationOptions.Compact));
        }

        public string ToString(StringGenerationContext ctx)
        {
            if (HasName())
            {
                string sname = Name;
                if (!Lexer.IsValidIdentifier(sname))
                    sname = Lexer.EscapeString(sname);
                return $"{sname} = {ToStringInternal(ctx)}";
            }

            return ToStringInternal(ctx);
        }

        public abstract Element Copy();
        protected virtual void CopyTo(Element other)
        {
            if (HasName())
                other.Name = Name;
        }

        public static Element NamedElement(string name, Element element)
        {
            element.Name = name;
            return element;
        }
    }
}