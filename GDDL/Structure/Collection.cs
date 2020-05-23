using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GDDL.Config;

namespace GDDL.Structure
{
    public class Collection : Element, IList<Element>, IDictionary<string, Element>
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
            set => throw new InvalidOperationException("Cannot put elements by name.");
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

        public Collection()
        {
        }

        public Collection(string typeName)
        {
            TypeName = typeName;
        }

        public Collection(IEnumerable<Element> init)
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

        public void Put(string name, Element e)
        {
            e = e.WithName(name);
            contents.Add(e);
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
            return !contents.Any(a => a is Collection || a.HasName());
        }

        protected override void ToStringImpl(StringBuilder builder, StringGenerationContext ctx)
        {
            ctx.PushIndent();

            bool oneElementPerLine = !IsSimple() || contents.Count > ctx.Options.oneElementPerLineThreshold;

            if (HasTypeName())
            {
                builder.Append(typeName);
                if (ctx.Options.lineBreaksBeforeOpeningBrace == 0)
                    builder.Append(" ");
            }
            bool addBraces = ctx.IndentLevel > 0 || HasTypeName();
            if (addBraces)
            {
                if (oneElementPerLine && ctx.Options.lineBreaksBeforeOpeningBrace > 0)
                {
                    for (int i = 0; i < ctx.Options.lineBreaksBeforeOpeningBrace; i++)
                    {
                        builder.Append("\n");
                    }
                    ctx.AppendIndent(builder);
                }
                else if (ctx.Options.spacesBeforeOpeningBrace > 0)
                {
                    for (int i = 0; i < ctx.Options.spacesBeforeOpeningBrace; i++)
                    {
                        builder.Append(" ");
                    }
                }
                builder.Append("{");
                if (oneElementPerLine && ctx.Options.lineBreaksAfterOpeningBrace > 0)
                {
                    for (int i = 0; i < ctx.Options.lineBreaksAfterOpeningBrace; i++)
                    {
                        builder.Append("\n");
                    }
                }
                else if (ctx.Options.spacesAfterOpeningBrace > 0)
                {
                    for (int i = 0; i < ctx.Options.spacesAfterOpeningBrace; i++)
                    {
                        builder.Append(" ");
                    }
                }
                ctx.PushIndent();
                ctx.IncIndent();
            }

            bool first = true;
            foreach (var e in contents)
            {
                ctx.PushIndent();

                if (first && (!oneElementPerLine || ctx.Options.lineBreaksAfterOpeningBrace == 0))
                {
                    ctx.SetIndent(0);
                }
                else if (!first)
                {
                    builder.Append(",");
                    if (oneElementPerLine)
                    {
                        builder.Append("\n");
                    }
                    else if (ctx.Options.spacesBetweenElements > 0)
                    {
                        for (int i = 0; i < ctx.Options.spacesBetweenElements; i++)
                        {
                            builder.Append(" ");
                        }
                    }

                    if (!oneElementPerLine)
                        ctx.SetIndent(0);
                }

                e.ToStringWithName(builder, ctx);

                first = false;
                ctx.PopIndent();
            }

            if (addBraces)
            {
                ctx.PopIndent();
                if (oneElementPerLine && ctx.Options.lineBreaksBeforeClosingBrace > 0)
                {
                    for (int i = 0; i < ctx.Options.lineBreaksBeforeClosingBrace; i++)
                    {
                        builder.Append("\n");
                    }
                    ctx.AppendIndent(builder);
                }
                else if (ctx.Options.spacesBeforeClosingBrace > 0)
                {
                    for (int i = 0; i < ctx.Options.spacesBeforeClosingBrace; i++)
                    {
                        builder.Append(" ");
                    }
                }
                builder.Append("}");
                if (oneElementPerLine && ctx.Options.lineBreaksAfterClosingBrace > 0)
                {
                    for (int i = 0; i < ctx.Options.lineBreaksAfterClosingBrace; i++)
                    {
                        builder.Append("\n");
                    }
                }
                else if (ctx.Options.spacesAfterClosingBrace > 0)
                {
                    for (int i = 0; i < ctx.Options.spacesAfterClosingBrace; i++)
                    {
                        builder.Append(" ");
                    }
                }
            }

            ctx.PopIndent();
        }

        public override Element Copy()
        {
            var b = new Collection();
            CopyTo(b);
            return b;
        }

        protected override void CopyTo(Element other)
        {
            base.CopyTo(other);
            if (!(other is Collection))
                throw new ArgumentException("CopyTo for invalid type", nameof(other));
            Collection b = (Collection)other;
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

        public IEnumerable<Collection> ByType(string type)
        {
            return contents.OfType<Collection>().Where(e => e.TypeName == type);
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