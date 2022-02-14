using GDDL.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GDDL.Structure
{
    public sealed class GddlMap : GddlElement<GddlMap>, IDictionary<string, GddlElement>
    {
        #region API
        public static GddlMap Empty()
        {
            return new GddlMap();
        }

        public static GddlMap Of(params KeyValuePair<string, GddlElement>[] initial)
        {
            return new GddlMap(initial);
        }

        public static GddlMap CopyOf(IEnumerable<KeyValuePair<string, GddlElement>> initial)
        {
            return new GddlMap(initial);
        }

        public override bool IsMap => true;
        public override GddlMap AsMap => this;

        public string TrailingComment { get; set; }
        public bool HasTrailingComment => !string.IsNullOrEmpty(TrailingComment);

        public bool HasTypeName => !string.IsNullOrEmpty(TypeName);
        public string TypeName { get; set; }

        public int Count => contents.Count;

        public bool IsReadOnly => false;

        public bool IsEmpty => contents.Count == 0;
        public bool IsSimple => !contents.Values.Any(a => a is GddlMap || a is GddlList);

        public ICollection<string> Keys => contents.Keys;

        public ICollection<GddlElement> Values => contents.Values;

        public GddlElement this[string name]
        {
            get => contents[name];
            set
            {
                var prev = contents[name];
                if (!ReferenceEquals(prev, value))
                {
                    contents[name] = value;
                    OnRemove(prev);
                    OnAdd(value);
                }
            }
        }

        public GddlMap()
        {
        }

        public GddlMap(IEnumerable<KeyValuePair<string, GddlElement>> init)
        {
            AddRange(init);
        }

        public GddlMap WithTypeName(string typeName)
        {
            TypeName = typeName;
            return this;
        }

        public void Add(string name, GddlElement e)
        {
            contents.Add(name, e);
            OnAdd(e);
        }

        public void AddRange(IEnumerable<KeyValuePair<string, GddlElement>> c)
        {
            foreach (var (name, e) in c)
            {
                Add(name, e);
            }
        }

        public bool Remove(string name)
        {
            if (contents.TryGetValue(name, out var existing))
            {
                OnRemove(existing);
            }
            return contents.Remove(name);
        }

        public void Clear()
        {
            foreach (var e in contents.Values) OnRemove(e);
            contents.Clear();
        }

        public override int GetFormattingComplexity()
        {
            return 2 + contents.Values.Sum(e => e.GetFormattingComplexity());
        }
        #endregion

        #region Implementation
        private readonly LinkedDictionary<string, GddlElement> contents = new LinkedDictionary<string, GddlElement>();

        private void OnAdd(GddlElement e)
        {
            if (e.Parent != null) throw new InvalidOperationException("The element is already assigned to a collection.");
            e.Parent = this;
        }

        private void OnRemove(GddlElement e)
        {
            e.Parent = null;
        }

        protected internal IEnumerable<string> KeysOf(GddlElement value)
        {
            return contents.Where(kv => ReferenceEquals(kv.Value, value)).Select(kv => kv.Key);
        }

        #endregion

        #region IDictionary Extras
        public IEnumerator<KeyValuePair<string, GddlElement>> GetEnumerator()
        {
            return contents.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)contents).GetEnumerator();
        }

        public bool ContainsKey(string name)
        {
            return contents.ContainsKey(name);
        }

        public bool ContainsValue(GddlElement item)
        {
            return contents.ContainsValue(item);
        }


        public bool TryGetValue(string key, out GddlElement value)
        {
            return contents.TryGetValue(key, out value);
        }

        void ICollection<KeyValuePair<string, GddlElement>>.Add(KeyValuePair<string, GddlElement> item)
        {
            Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<string, GddlElement>>.Contains(KeyValuePair<string, GddlElement> item)
        {
            return contents.Contains(item);
        }

        void ICollection<KeyValuePair<string, GddlElement>>.CopyTo(KeyValuePair<string, GddlElement>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, GddlElement>>)contents).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, GddlElement>>.Remove(KeyValuePair<string, GddlElement> item)
        {
            if (contents.TryGetValue(item.Key, out var existing) && Equals(existing, item.Value))
            {
                OnRemove(existing);
            }
            return ((ICollection<KeyValuePair<string, GddlElement>>)contents).Remove(item);
        }
        #endregion

        #region Element Implementation
        public override GddlMap CopyInternal()
        {
            var collection = new GddlMap();
            CopyTo(collection);
            return collection;
        }

        protected override void CopyTo(GddlMap other)
        {
            base.CopyTo(other);
            foreach (var (name,e) in contents)
            {
                other.Add(name, e.Copy());
            }
        }

        public override void Resolve(GddlElement root)
        {
            foreach (var el in contents.Values)
            {
                el.Resolve(root);
            }
        }

        public override GddlElement Simplify()
        {
            foreach (var name in contents.Keys.ToList())
            {
                contents[name] = contents[name].Simplify();
            }

            return this;
        }
        #endregion

        #region Equality
        public override bool Equals(object other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other == null || GetType() != other.GetType()) return false;
            return EqualsImpl((GddlMap)other);
        }

        public override bool Equals(GddlMap other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other == null) return false;
            return EqualsImpl(other);
        }

        private bool EqualsImpl(GddlMap other)
        {
            if (!base.EqualsImpl(other))
                return false;

            if (!Equals(TypeName, other.TypeName))
                return false;

            if (contents.Count != other.contents.Count)
                return false;

            foreach (var (k,v) in contents)
            {
                if (!other.contents.TryGetValue(k, out var v2))
                    return false;

                if (!Equals(v, v2))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), contents, TypeName);
        }
        #endregion
    }
}