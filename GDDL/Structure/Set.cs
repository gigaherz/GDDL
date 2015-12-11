using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GDDL.Structure
{
    public class Set : Element, IList<Element>
    {
        private readonly List<Element> contents = new List<Element>();
        private readonly Dictionary<string, Element> names = new Dictionary<string, Element>();

        internal ICollection<Element> Contents
        {
            get { return contents.AsReadOnly(); }
        }

        public Element this[int index]
        {
            get { return contents[index]; }
            set
            {
                var old = contents[index];
                if (old.HasName)
                    names.Remove(old.Name);
                contents[index] = value;
                if (value.HasName)
                    names.Add(value.Name, value);
            }
        }

        public Element this[string name]
        {
            get { return names[name]; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public int Count
        {
            get { return contents.Count; }
        }

        public string TypeName
        {
            get; set;
        }

        public bool HasTypeName
        {
            get { return !string.IsNullOrEmpty(TypeName); }
        }

        internal Set()
        {
            contents = new List<Element>();
        }

        internal Set(IEnumerable<Element> init)
        {
            contents = new List<Element>();
            contents.AddRange(init);
        }

        IEnumerator IEnumerable.GetEnumerator() { return contents.GetEnumerator(); }

        public IEnumerator<Element> GetEnumerator() { return contents.GetEnumerator(); }

        public bool Contains(Element e)
        {
            return contents.Contains(e);
        }

        public int IndexOf(Element e)
        {
            return contents.IndexOf(e);
        }

        internal void Append(Element e)
        {
            contents.Add(e);
            if (e.HasName)
                names.Add(e.Name, e);
        }

        public void Add(Element e)
        {
            contents.Add(e);
            if (e.HasName)
                names.Add(e.Name, e);
        }

        public void Insert(int before, Element e)
        {
            contents.Insert(before, e);
            if (e.HasName)
                names.Add(e.Name, e);
        }

        public void AddRange(IEnumerable<Element> set)
        {
            foreach (var e in set)
                Add(e);
        }

        public bool Remove(Element e)
        {
            var r = contents.Remove(e);
            if (e.HasName)
                names.Remove(e.Name);
            return r;
        }

        public void RemoveAt(int index)
        {
            var e = contents[index];
            contents.RemoveAt(index);
            if (e.HasName)
                names.Remove(e.Name);
        }

        public void Clear()
        {
            contents.Clear();
            names.Clear();
        }

        public bool IsSimple()
        {
            return
                !(contents.Any(a => a is Set) ||
                  contents.Where(a => a.HasName).Any(a => a is Set));
        }

        public void CopyTo(Element[] dest, int offset)
        {
            contents.CopyTo(dest, offset);
        }

        protected override string ToStringInternal()
        {
            if (HasTypeName)
                return string.Format("{0} {1}", TypeName, ToStringInternal(true));
            return ToStringInternal(true);
        }

        protected virtual string ToStringInternal(bool addBraces)
        {
            return string.Format(addBraces ? "{{{0}}}" : "{0}", string.Join(", ", contents));
        }

        protected override string ToStringInternal(StringGenerationContext ctx)
        {
            var addBraces = ctx.IndentLevel > 0;
            int tabsToGen = ctx.IndentLevel - 1;

            string tabs1 = "";
            for (int i = 0; i < tabsToGen; i++)
                tabs1 += "  ";

            string tabs2 = tabs1;

            if (addBraces)
                tabs2 = "  " + tabs1;

            var nice = ((ctx.Options & StringGenerationOptions.Nice) == StringGenerationOptions.Nice)
                       && (!IsSimple() || contents.Count > 10);

            ctx.IndentLevel++;

            string result = nice
                                ? string.Join(", " + Environment.NewLine, contents.Select(a => tabs2 + a.ToString(ctx)))
                                : string.Join(", ", contents.Select(a => a.ToString(ctx)));

            if (addBraces)
            {
                result = string.Format(nice ? "{{{0}{2}{0}{1}}}" : "{{{2}}}", Environment.NewLine, tabs1, result);
            }

            if (HasTypeName)
            {
                result = string.Format("{0} {1}", TypeName, result);
            }

            ctx.IndentLevel--;

            return result;
        }

        internal override void Resolve(Element root)
        {
            foreach (var el in contents)
            {
                el.Resolve(root);
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

        public bool TryGetByName(string name, out Element elm)
        {
            return names.TryGetValue(name, out elm);
        }

        public IEnumerable<Element> ByName(string name)
        {
            return contents.Where(c => string.CompareOrdinal(c.Name, name) == 0);
        }

        public IEnumerable<Set> ByType(string typeName)
        {
            return contents.OfType<Set>().Where(e => string.CompareOrdinal(e.TypeName, typeName) == 0);
        }
    }
}