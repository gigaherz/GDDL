using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GDDL.Util;

namespace GDDL.Structure
{
    public class Collection : Element, IList<Element>, IEquatable<Collection>
    {
        // Factory Methods
        public static Collection Empty()
        {
            return new Collection();
        }
        public static Collection Of(params Element[] initial)
        {
            return new Collection(initial);
        }

        public static Collection CopyOf(IEnumerable<Element> initial)
        {
            return new Collection(initial);
        }

        // Implementation
        private readonly List<Element> contents = new List<Element>();
        private readonly MultiMap<string, Element> names = new MultiMap<string, Element>();

        private string typeName;

        public bool HasTypeName => !string.IsNullOrEmpty(typeName);
        public string TypeName
        {
            get => typeName;
            set
            {
                if (!Utility.IsValidIdentifier(value))
                    throw new ArgumentException("Type value must be a valid identifier");
                typeName = value;
            }
        }

        public int Count => contents.Count;

        public bool IsReadOnly => false;

        public bool IsEmpty => contents.Count == 0;
        public bool IsSimple => !contents.Any(a => a is Collection || a.HasName);

        public Optional<Element> this[string name]
        {
            get => TryGetValue(name, out var e) ? Optional.Of(e) : Optional.Empty<Element>();
        }

        public Element this[int index]
        {
            get => contents[index];
            set
            {
                var old = contents[index];
                if (old.HasName)
                    names.Remove(old.Name, old);
                contents[index] = value;
                if (value.HasName)
                    names.Add(value.Name, value);
            }
        }

        private Collection()
        {
        }

        private Collection(IEnumerable<Element> init)
        {
            AddRange(init);
        }

        public Collection WithTypeName(string typeName)
        {
            TypeName = typeName;
            return this;
        }

        public void Add(Element e)
        {
            contents.Add(e);
            if (e.HasName)
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
            if (e.HasName)
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
            if (e.HasName)
                names.Remove(e.Name, e);
            return r;
        }

        public void RemoveAt(int index)
        {
            var e = contents[index];
            contents.RemoveAt(index);
            if (e.HasName)
                names.Remove(e.Name, e);
        }

        public int IndexOf(Element o)
        {
            return contents.IndexOf(o);
        }

        public void Clear()
        {
            contents.Clear();
            names.Clear();
        }

        public override Element Copy()
        {
            return CopyCollection();
        }

        public Collection CopyCollection()
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
            return contents.Where(t => t.HasName && t.Name == elementName);
        }

        public IEnumerable<Collection> ByType(string type)
        {
            return contents.OfType<Collection>().Where(e => e.TypeName == type);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out Element result)
        {
            var n = names[key];
            if (n.Count > 0)
            {
                result = n.First();
                return true;
            }
            else
            {
                result = null;
                return false;
            }
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

        public bool ContainsKey(string key)
        {
            return names[key].Count > 0;
        }

        public bool RemoveAll(string key)
        {
            var items = names[key];
            if (items.Count == 0)
                return false;
            items.Clear();
            return true;
        }

        #region Equality
        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if (obj == null || GetType() != obj.GetType()) return false;
            return obj is Collection other ? EqualsImpl(other) : false;
        }

        public bool Equals(Collection other)
        {
            if (other == this) return true;
            if (other == null) return false;
            return EqualsImpl(other);
        }

        protected bool EqualsImpl(Collection other)
        {
            if (!base.EqualsImpl(other)) return false;
            return Enumerable.SequenceEqual(contents, other.contents) &&
                Enumerable.SequenceEqual(names, other.names) &&
                Equals(typeName, other.typeName);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), contents, names, typeName);
        }
        #endregion
    }
}