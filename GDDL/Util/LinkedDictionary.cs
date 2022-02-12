using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GDDL.Util
{
    internal class LinkedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        public LinkedDictionary()
        {
        }

        public LinkedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> values)
            : this()
        {
            AddRange(values);
        }

        public int Count => entries.Count;
        public bool IsReadOnly => false;

        public TValue this[TKey key]
        {
            get => table[key].Value.Value;
            set
            {
                if (table.TryGetValue(key, out var node))
                {
                    node.Value = KeyValuePair.Create(key, value);
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        private KeyView _keys;
        public ICollection<TKey> Keys => _keys ??= new KeyView(this);

        private ValueView _values;
        public ICollection<TValue> Values => _values ??= new ValueView(this);

        public void Add(TKey key, TValue value)
        {
            if (table.ContainsKey(key))
                throw new ArgumentException("An element with the same key already exists in the dictionary.", nameof(key));
            var node = entries.AddLast(KeyValuePair.Create(key, value));
            table.Add(key, node);
        }

        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> values)
        {
            foreach (var kv in values)
            {
                Add(kv);
            }
        }

        public bool ContainsKey(TKey key)
        {
            return table.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            foreach (var e in entries)
            {
                if (Equals(e.Value, value)) return true;
            }
            return false;
        }

        public bool Remove(TKey key)
        {
            if (table.TryGetValue(key, out var node))
            {
                entries.Remove(node);
            }
            return table.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (table.TryGetValue(key, out var node))
            {
                value = node.Value.Value;
                return true;
            }
            value = default;
            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (table.ContainsKey(item.Key))
                throw new ArgumentException("An element with the same key already exists in the dictionary.", nameof(item));
            var node = entries.AddLast(item);
            table.Add(item.Key, node);
        }

        public void Clear()
        {
            entries.Clear();
            table.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return entries.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            entries.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (table.TryGetValue(item.Key, out var node))
            {
                if (Equals(node.Value, item))
                {
                    entries.Remove(node);
                    table.Remove(item.Key);
                    return true;
                }
            }
            return false;
        }

        private readonly LinkedList<KeyValuePair<TKey, TValue>> entries = new LinkedList<KeyValuePair<TKey, TValue>>();
        private readonly Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> table = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();

        private class KeyView : ICollection<TKey>, IReadOnlyCollection<TKey>
        {
            private readonly LinkedDictionary<TKey, TValue> owner;

            public int Count => owner.entries.Count;
            public bool IsReadOnly => true;

            public KeyView(LinkedDictionary<TKey, TValue> owner)
            {
                this.owner = owner;
            }

            public IEnumerator<TKey> GetEnumerator()
            {
                return owner.entries.Select(e => e.Key).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(TKey item)
            {
                throw new NotSupportedException("Cannot modify this collection");
            }

            public void Clear()
            {
                owner.Clear();
            }

            public bool Contains(TKey item)
            {
                return item != null && owner.ContainsKey(item);
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                int space = array.Length - arrayIndex;
                if (owner.Count > space)
                    throw new ArgumentOutOfRangeException(nameof(array), "The array is too small");
                int i = arrayIndex;
                foreach(var entry in owner.entries)
                {
                    array[i++] = entry.Key;
                }
            }

            public bool Remove(TKey item)
            {
                return item != null && owner.Remove(item);
            }
        }

        private class ValueView : ICollection<TValue>, IReadOnlyCollection<TValue>
        {
            private readonly LinkedDictionary<TKey, TValue> owner;

            public int Count => owner.entries.Count;
            public bool IsReadOnly => true;

            public ValueView(LinkedDictionary<TKey, TValue> owner)
            {
                this.owner = owner;
            }

            public IEnumerator<TValue> GetEnumerator()
            {
                return owner.entries.Select(e => e.Value).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(TValue item)
            {
                throw new NotSupportedException("Cannot modify this collection");
            }

            public void Clear()
            {
                throw new NotSupportedException("Cannot modify this collection");
            }

            public bool Contains(TValue item)
            {
                return item != null && owner.ContainsValue(item);
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                int space = array.Length - arrayIndex;
                if (owner.Count > space)
                    throw new ArgumentOutOfRangeException(nameof(array), "The array is too small");
                int i = arrayIndex;
                foreach (var entry in owner.entries)
                {
                    array[i++] = entry.Value;
                }
            }

            public bool Remove(TValue item)
            {
                throw new NotSupportedException("Cannot modify this collection");
            }
        }
    }
}
