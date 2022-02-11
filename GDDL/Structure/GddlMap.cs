using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GDDL.Util;

namespace GDDL.Structure
{
    public sealed class GddlMap : Element<GddlMap>, IDictionary<string, GddlElement>, IEquatable<GddlMap>
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
        public bool IsSimple => !contents.Values.Any(a => a is GddlMap || a is GddlList);

        public ICollection<string> Keys => contents.Keys;

        public ICollection<GddlElement> Values => contents.Values;

        public GddlElement this[string name]
        {
            get => contents[name];
            set => contents[name] = value;
        }

        public GddlMap WithTypeName(string typeName)
        {
            TypeName = typeName;
            return this;
        }

        public void Add(string name, GddlElement e)
        {
            contents.Add(name, e);
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
            return contents.Remove(name);
        }

        public void Clear()
        {
            contents.Clear();
        }
        #endregion

        #region Implementation
        private readonly Dictionary<string, GddlElement> contents = new Dictionary<string, GddlElement>();

        private string typeName;

        private GddlMap()
        {
        }

        private GddlMap(IEnumerable<KeyValuePair<string, GddlElement>> init)
        {
            AddRange(init);
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
            ((ICollection<KeyValuePair<string, GddlElement>>)contents).Add(item);
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
            foreach (var name in contents.Keys)
            {
                contents[name] = contents[name].Simplify();
            }

            return this;
        }
        #endregion

        #region Equality
        public override bool Equals(object other)
        {
            if (other == this) return true;
            if (other == null || GetType() != other.GetType()) return false;
            return EqualsImpl((GddlMap)other);
        }

        public override bool Equals(GddlMap other)
        {
            if (other == this) return true;
            if (other == null) return false;
            return EqualsImpl(other);
        }

        private bool EqualsImpl(GddlMap other)
        {
            return base.EqualsImpl(other) &&
                Equals(contents, other.contents) &&
                Equals(typeName, other.typeName);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), contents, typeName);
        }
        #endregion
    }
}