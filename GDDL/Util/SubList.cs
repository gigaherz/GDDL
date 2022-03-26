using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GDDL.Util
{
    public class SubList<T> : IReadOnlyList<T>
    {
        public List<T> Parent { get; }
        public int Start { get; }
        public int Count { get; }

        public SubList(List<T> contents, int start, int count)
        {
            Parent = contents;
            Start = start;
            Count = count;
        }

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