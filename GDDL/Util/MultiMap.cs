using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GDDL.Util
{
    public class MultiMap<TKey, TValue>(Func<IDictionary<TKey, ICollection<TValue>>> storageFactory,
        Func<ICollection<TValue>> collectionFactory) : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly IDictionary<TKey, ICollection<TValue>> storage = storageFactory();

        public MultiMap() : this(() => new Dictionary<TKey, ICollection<TValue>>(), () => new HashSet<TValue>())
        {
        }

        [return: MaybeNull]
        private bool TryGetValues(TKey key, out ICollection<TValue> values)
        {
            return storage.TryGetValue(key, out values);
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

        public ICollection<TValue> this[TKey key] => GetOrCreate(key);

        private bool Contains(TKey key, TValue value)
        {
            return TryGetValues(key, out var collection) && collection.Contains(value);
        }

        public bool Remove(TKey key, TValue value)
        {
            return TryGetValues(key, out var collection) && collection.Remove(value);
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

        public IEnumerator<KeyValuePair<TKey, ICollection<TValue>>> GetKeywiseEnumerator()
        {
            return storage.GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetValuewiseEnumerator()
        {
            foreach (var ks in storage)
            {
                foreach (var v in ks.Value)
                {
                    yield return new KeyValuePair<TKey, TValue>(ks.Key, v);
                }
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => GetValuewiseEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetValuewiseEnumerator();
    }
}