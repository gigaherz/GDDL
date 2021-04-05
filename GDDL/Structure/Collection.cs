using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GDDL.Util;

namespace GDDL.Structure
{
    public sealed class Collection : Element<Collection>, IList<Element>, IEquatable<Collection>
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

        private void OnAdd(Element e)
        {
            if (e.HasName)
                names.Add(e.Name, e);
            e.ParentInternal = this;
        }

        private void OnRemove(Element e)
        {
            if (e.HasName)
                names.Remove(e.Name, e);
            e.ParentInternal = null;
        }

        public Collection WithTypeName(string typeName)
        {
            TypeName = typeName;
            return this;
        }

        public void Add(Element e)
        {
            contents.Add(e);
            OnAdd(e);
        }

        public void Put(string name, Element e)
        {
            e = e.WithName(name);
            contents.Add(e);
            OnAdd(e);
        }

        public void Insert(int before, Element e)
        {
            contents.Insert(before, e);
            OnAdd(e);
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
            OnRemove(e);
            return r;
        }

        public void RemoveAt(int index)
        {
            var e = contents[index];
            contents.RemoveAt(index);
            OnRemove(e);
        }

        public void SetName(Element e, string name)
        {
            string currentName = e.Name;
            if (!Equals(currentName, name))
            {
                OnRemove(e);
                e.Name = name;
                OnAdd(e);
            }
        }

        public int IndexOf(Element o)
        {
            return contents.IndexOf(o);
        }

        public void Clear()
        {
            foreach(var e in contents) e.ParentInternal = null;
            contents.Clear();
            names.Clear();
        }

        public override Collection CopyInternal()
        {
            var collection = new Collection();
            CopyTo(collection);
            return collection;
        }

        protected override void CopyTo(Collection other)
        {
            base.CopyTo(other);
            foreach (var e in contents)
            {
                other.Add(e.Copy());
            }
        }


        public override void Resolve(Element root, [MaybeNull] Collection parent)
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
            return names[elementName];
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
            bool removed = false;
            var items = names[key];
            foreach (var it in items)
            {
                removed |= Remove(it);
            }
            return true;
        }

        #region Equality
        public override bool Equals(object other)
        {
            if (other == this) return true;
            if (other == null || GetType() != other.GetType()) return false;
            return EqualsImpl((Collection)other);
        }

        public override bool Equals(Collection other)
        {
            if (other == this) return true;
            if (other == null) return false;
            return EqualsImpl(other);
        }

        private bool EqualsImpl(Collection other)
        {
            return base.EqualsImpl(other) &&
                Enumerable.SequenceEqual(contents, other.contents) &&
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