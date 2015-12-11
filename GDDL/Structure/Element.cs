using System.Collections.Generic;
using System.Globalization;

namespace GDDL.Structure
{
    public abstract class Element
    {
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public bool HasName { get { return !string.IsNullOrEmpty(Name); } }

        public bool IsSet { get { return this is Set; } }
        public Set AsSet { get { return (Set)this; } }

        public bool IsValue { get { return this is Value; } }
        public Value AsValue { get { return (Value)this; } }

        // Removes single-element non-named sets and backreferences
        public virtual Element Simplify()
        {
            return this;
        }

        internal virtual void Resolve(Element root)
        {
        }

        public virtual bool IsResolved
        {
            get
            {
                return true;
            }
        }

        public virtual Element ResolvedValue
        {
            get
            {
                return this;
            }
        }

        protected abstract string ToStringInternal();
        protected abstract string ToStringInternal(StringGenerationContext ctx);

        public override string ToString()
        {
            if (HasName)
            {
                return string.Format("{0} = {1}", Name, ToStringInternal());
            }

            return ToStringInternal();
        }
        public string ToString(StringGenerationContext ctx)
        {
            if (HasName)
            {
                return string.Format("{0} = {1}", Name, ToStringInternal(ctx));
            }

            return ToStringInternal(ctx);
        }



        // Factory methods
        public static Set Set(params Element[] initial)
        {
            return new Set(initial);
        }

        public static Set Set(IEnumerable<Element> initial)
        {
            return new Set(initial);
        }

        public static Backreference Backreference(bool rooted, string name)
        {
            return new Backreference(rooted, name);
        }

        public static Value Null()
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

        public static Value IntValue(string text)
        {
            return new Value(long.Parse(text, CultureInfo.InvariantCulture));
        }

        public static Value IntValue(string text, int _base)
        {
            return new Value(long.Parse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
        }

        public static Value FloatValue(double num)
        {
            return new Value(num);
        }

        public static Value FloatValue(string text)
        {
            return new Value(double.Parse(text, CultureInfo.InvariantCulture));
        }

        public static Value StringValue(string text)
        {
            if (text[0] == '"')
                return new Value(Value.UnescapeString(text));

            return new Value(text);
        }
    }
}
