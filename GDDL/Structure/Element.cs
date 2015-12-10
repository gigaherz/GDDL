using System.Collections.Generic;
using System.Globalization;

namespace GDDL.Structure
{
    public abstract class Element
    {
        public abstract override string ToString();

        // Removes single-element non-named sets and backreferences
        public virtual Element Simplify()
        {
            return this;
        }

        internal virtual void Resolve(Set rootSet)
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

        public abstract string ToString(StringGenerationContext ctx);

        // Factory methods
        public static Set Set(params Element[] initial) 
        {
            return new Set(initial); 
        }

        public static Set Set(IEnumerable<Element> initial)
        {
            return new Set(initial);
        }

        public static RootSet RootSet(Set copyFrom)
        {
            return new RootSet(copyFrom);
        }

        public static TypedSet TypedSet(string name)
        {
            return new TypedSet(name);
        }

        public static TypedSet TypedSet(string name, Set copyFrom) 
        {
            return new TypedSet(name, copyFrom); 
        }

        public static NamedElement NamedElement(string name, Element value)
        {
            return new NamedElement(name, value);
        }

        public static Backreference Backreference(bool rooted, string name)
        {
            return new Backreference(rooted, name);
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
            return new Value(long.Parse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture)); 
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
