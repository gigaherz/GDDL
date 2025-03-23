using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GDDL.Util
{
    public sealed class SubList<T>(List<T> parent, int start, int count) : IReadOnlyList<T>
    {
        public List<T> Parent => parent;
        public int Start => start;
        public int Count => count;

        public IEnumerator<T> GetEnumerator()
        {
            return Parent.Skip(Start).Take(Count).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T this[int index] => Parent[Start + index];

        public T this[Index index] => Parent[Start + (index.IsFromEnd ? Count - index.Value : index.Value)];

        public IReadOnlyList<T> this[Range range]
        {
            get
            {
                var (start, length) = range.GetOffsetAndLength(Count);
                return new SubList<T>(Parent, Start + start, length);
            }
        }
    }
}