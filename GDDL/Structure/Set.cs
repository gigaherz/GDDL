using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GDDL.Config;

namespace GDDL.Structure
{
    public class Set : Element, IList<Element>, IDictionary<string, Element>
    {
        private readonly List<Element> contents = new List<Element>();
        private readonly Dictionary<string, Element> names = new Dictionary<string, Element>();

        private string typeName;

        public string TypeName
        {
            get => typeName;
            set
            {
                if (!Lexer.IsValidIdentifier(value))
                    throw new ArgumentException("Type value must be a valid identifier");
                typeName = value;
            }
        }

        public int Count => contents.Count;

        public bool IsReadOnly => false;

        public Element this[string name]
        {
            get => names[name];
            set => throw new InvalidOperationException();
        }

        public Element this[int index]
        {
            get => contents[index];
            set
            {
                var old = contents[index];
                if (old.HasName())
                    names.Remove(old.Name);
                contents[index] = value;
                if (value.HasName())
                    names.Add(value.Name, value);
            }
        }

        public Set()
        {
        }

        public Set(string typeName)
        {
            TypeName = typeName;
        }

        public Set(IEnumerable<Element> init)
        {
            contents.AddRange(init);
        }

        public bool HasTypeName()
        {
            return TypeName != null;
        }

        public bool IsEmpty()
        {
            return contents.Count == 0;
        }

        public void Add(Element e)
        {
            contents.Add(e);
            if (e.HasName())
                names.Add(e.Name, e);
        }

        public void Insert(int before, Element e)
        {
            contents.Insert(before, e);
            if (e.HasName())
                names.Add(e.Name, e);
        }

        public void AddRange(IEnumerable<Element> c)
        {
            foreach (var e in c)
            {
                Add(e);
            }
        }

        public bool Remove(Element e)
        {
            bool r = contents.Remove(e);
            if (e.HasName())
                names.Remove(e.Name);
            return r;
        }

        public void RemoveAt(int index)
        {
            var e = contents[index];
            contents.RemoveAt(index);
            if (e.HasName())
                names.Remove(e.Name);
        }

        public int IndexOf(Element o)
        {
            return contents.IndexOf(o);
        }

        void ICollection<KeyValuePair<string, Element>>.Add(KeyValuePair<string, Element> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            contents.Clear();
            names.Clear();
        }

        bool ICollection<KeyValuePair<string, Element>>.Contains(KeyValuePair<string, Element> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, Element>>.CopyTo(KeyValuePair<string, Element>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool IsSimple()
        {
            return !contents.Any(a => a is Set || a.HasName());
        }

        protected override string ToStringInternal(StringGenerationContext ctx)
        {
            bool addBraces = ctx.IndentLevel > 0;
            int tabsToGen = ctx.IndentLevel - (addBraces ? 0 : 1);

            var tabs0 = new StringBuilder();
            for (int i = 0; i < tabsToGen; i++)
            {
                tabs0.Append("  ");
            }
            var tabs1 = tabs0.ToString();
            if (addBraces) tabs0.Append("  ");
            var tabs2 = tabs0.ToString();

            var builder = new StringBuilder();

            bool nice = ctx.Options == StringGenerationOptions.Nice;
            bool simple = IsSimple() && contents.Count <= 10;

            int verbosity = 0;
            if (nice && simple) verbosity = 1;
            else if (nice) verbosity = 2;

            ctx.IndentLevel++;

            if (HasTypeName())
            {
                builder.Append(TypeName);
                builder.Append(" ");
            }
            if (addBraces)
            {
                switch (verbosity)
                {
                    case 0: builder.Append("{"); break;
                    case 1: builder.Append("{ "); break;
                    case 2: builder.Append("{\n"); break;
                }
            }

            bool first = true;
            foreach (var e in contents)
            {
                if (!first)
                {
                    switch (verbosity)
                    {
                        case 0: builder.Append(","); break;
                        case 1: builder.Append(", "); break;
                        case 2: builder.Append(",\n"); break;
                    }
                }
                if (verbosity == 2) builder.Append(tabs2);

                builder.Append(e.ToString(ctx));

                first = false;
            }

            if (addBraces)
            {
                switch (verbosity)
                {
                    case 0: builder.Append("}"); break;
                    case 1: builder.Append(" }"); break;
                    case 2: builder.Append("\n"); builder.Append(tabs1); builder.Append("}"); break;
                }
            }

            ctx.IndentLevel--;

            return builder.ToString();
        }

        public override Element Copy()
        {
            var b = new Set();
            CopyTo(b);
            return b;
        }

        protected override void CopyTo(Element other)
        {
            base.CopyTo(other);
            if (!(other is Set))
                throw new ArgumentException("CopyTo for invalid type", nameof(other));
            Set b = (Set)other;
            foreach (var e in contents) b.Add(e.Copy());
        }


        public override void Resolve(Element root, Element parent)
        {
            foreach (var el in contents)
            {
                el.Resolve(root, this);
            }
        }

        public override Element Simplify()
        {
            for (int i = 0; i < contents.Count; i++)
            {
                contents[i] = contents[i].Simplify();
            }

            return this;
        }

        public IEnumerable<Element> ByName(string elementName)
        {
            return contents.Where(t => t.HasName() && t.Name == elementName);
        }

        public IEnumerable<Set> ByType(string type)
        {
            return contents.OfType<Set>().Where(e => e.TypeName == type);
        }

        public IEnumerator<Element> GetEnumerator()
        {
            return contents.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)contents).GetEnumerator();
        }

        public bool Contains(Element item)
        {
            return contents.Contains(item);
        }

        public void CopyTo(Element[] array, int arrayIndex)
        {
            contents.CopyTo(array, arrayIndex);
        }

        public bool TryGetValue(string key, out Element value)
        {
            return names.TryGetValue(key, out value);
        }

        public bool ContainsKey(string key)
        {
            return names.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            Element value;
            return TryGetValue(key, out value) && Remove(value);
        }

        #region IDictionary explicit
        IEnumerator<KeyValuePair<string, Element>> IEnumerable<KeyValuePair<string, Element>>.GetEnumerator()
        {
            return names.GetEnumerator();
        }

        bool ICollection<KeyValuePair<string, Element>>.Remove(KeyValuePair<string, Element> item)
        {
            throw new NotImplementedException();
        }

        void IDictionary<string, Element>.Add(string key, Element value)
        {
            throw new NotImplementedException();
        }

        ICollection<string> IDictionary<string, Element>.Keys => names.Keys;

        ICollection<Element> IDictionary<string, Element>.Values
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
    }
}