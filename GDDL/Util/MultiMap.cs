using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace GDDL.Util
{
    public class MultiMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly IDictionary<TKey, ICollection<TValue>> storage;
        private readonly Func<ICollection<TValue>> collectionFactory;

        public MultiMap() : this(() => new Dictionary<TKey, ICollection<TValue>>(), () => new HashSet<TValue>())
        {
        }

        public MultiMap(Func<IDictionary<TKey, ICollection<TValue>>> storageFactory, Func<ICollection<TValue>> collectionFactory)
        {
            storage = storageFactory();
            this.collectionFactory = collectionFactory;
        }

        private Optional<ICollection<TValue>> GetOrEmpty(TKey key)
        {
            if (!storage.TryGetValue(key, out ICollection<TValue> value))
            {
                return Optional<ICollection<TValue>>.Empty;
            }
            return Optional.Of(value);
        }

        [return: MaybeNull]
        private ICollection<TValue> GetOrCreate(TKey key)
        {
            if (!storage.TryGetValue(key, out ICollection<TValue> value))
            {
                value = collectionFactory();
                storage.Add(key, value);
            }
            return value;
        }

        public ICollection<TValue> this[TKey key]
        {
            get
            {
                return GetOrEmpty(key).OrElseGet(() => new List<TValue>());
            }
        }

        private bool Contains(TKey key, TValue value)
        {
            return GetOrEmpty(key).Map(collection=>collection.Contains(value)).OrElse(false);
        }

        public bool Remove(TKey key, TValue value)
        {
            return GetOrEmpty(key).Map(collection=>collection.Remove(value)).OrElse(false);
        }

        public void Add(TKey key, TValue value)
        {
            if (Contains(key, value))
                return;
            GetOrCreate(key).Add(value);
        }

        public void Clear()
        {
            storage.Clear();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach(var ks in storage)
            {
                foreach(var v in ks.Value)
                {
                    yield return new KeyValuePair<TKey, TValue>(ks.Key, v);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
