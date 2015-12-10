using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GDDL.Structure
{
    public class Set : Element, IList<Element>
    {
        private readonly List<Element> contents = new List<Element>();
        private readonly Dictionary<string, NamedElement> names = new Dictionary<string, NamedElement>();
        
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
                if (old is NamedElement)
                    names.Remove((old as NamedElement).Name);
                contents[index] = value;
                if (value is NamedElement)
                    names.Add((value as NamedElement).Name, (value as NamedElement));
            }
        }

        public NamedElement this[string name] 
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
            if (e is NamedElement)
                names.Add((e as NamedElement).Name, (e as NamedElement));
        }

        public void Add(Element e)
        {
            contents.Add(e);
            if (e is NamedElement)
                names.Add((e as NamedElement).Name, (e as NamedElement));
        }
        
        public void Insert(int before, Element e)
        {
            contents.Insert(before, e);
            if (e is NamedElement)
                names.Add((e as NamedElement).Name, (e as NamedElement));
        }

        public void AddRange(IEnumerable<Element> set)
        {
            foreach(var e in set)
                Add(e);
        }

        public bool Remove(Element e)
        {
            var r = contents.Remove(e);
            if (e is NamedElement)
                names.Remove((e as NamedElement).Name);
            return r;
        }

        public void RemoveAt(int index)
        {
            var e = contents[index];
            contents.RemoveAt(index);
            if (e is NamedElement)
                names.Remove((e as NamedElement).Name);
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
                  contents.Select(a => a as NamedElement).Where(a => a != null).Any(a => a.Value is Set));
        }

        public void CopyTo(Element[] dest, int offset)
        {
            contents.CopyTo(dest, offset);
        }

        public override string ToString() 
        {
            return ToString(true);
        }

        public virtual string ToString(bool addBraces)
        {
            return string.Format(addBraces ? "{{{0}}}" : "{0}", string.Join(", ", contents));
        }

        public override string ToString(StringGenerationContext ctx)
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

            if(addBraces)
            {
                result = string.Format(nice ? "{{{0}{2}{0}{1}}}" : "{{{2}}}", Environment.NewLine, tabs1, result);
            }
            
            ctx.IndentLevel--;

            return result;
        }

        internal override void Resolve(Set rootSet)
        {
            foreach (var el in contents)
            {
                el.Resolve(rootSet);
            }
        }

        public override Element Simplify()
        {
            for(int i=0;i<contents.Count;i++)
            {
                contents[i] = contents[i].Simplify();
            }

            //if (contents.Count == 1) 
            //    return contents[0];
            
            return this;
        }

        public bool TryGetByName(string name, out NamedElement elm)
        {
            return names.TryGetValue(name, out elm);
        }

        public IEnumerable<Element> ByName(string name)
        {
            return from c in contents let n = (c as INamed) where n != null && n.Name == name select c;
        }

        public IEnumerable<TypedSet> ByType(string typeName)
        {
            return contents.OfType<TypedSet>().Where(e => string.CompareOrdinal(e.TypeName, typeName) == 0);
        }
    }
}