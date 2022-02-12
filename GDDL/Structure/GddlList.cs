using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GDDL.Util;

namespace GDDL.Structure
{
    public sealed class GddlList : GddlElement<GddlList>, IList<GddlElement>
    {
        #region API
        public static GddlList Empty()
        {
            return new GddlList();
        }
        public static GddlList Of(params GddlElement[] initial)
        {
            return new GddlList(initial);
        }

        public static GddlList CopyOf(IEnumerable<GddlElement> initial)
        {
            return new GddlList(initial);
        }

        public override bool IsList => true;
        public override GddlList AsList => this;

        public string TrailingComment { get; set; }
        public bool HasTrailingComment => !string.IsNullOrEmpty(TrailingComment);

        public int Count => contents.Count;

        public bool IsReadOnly => false;

        public bool IsEmpty => contents.Count == 0;

        public bool IsSimple => !contents.Any(a => a is GddlMap || a is GddlList);

        public GddlElement this[int index]
        {
            get => contents[index];
            set {
                var prev = contents[index];
                if (!ReferenceEquals(prev, value))
                {
                    contents[index] = value;
                    OnRemove(prev);
                    OnAdd(value);
                }
            }
        }

        public GddlList()
        {
        }

        public GddlList(IEnumerable<GddlElement> init)
        {
            AddRange(init);
        }

        public void Add(GddlElement e)
        {
            contents.Add(e);
            OnAdd(e);
        }

        public void Insert(int before, GddlElement e)
        {
            contents.Insert(before, e);
            OnAdd(e);
        }

        public void AddRange(IEnumerable<GddlElement> c)
        {
            foreach (var e in c)
            {
                Add(e);
            }
        }

        public bool Remove(GddlElement e)
        {
            var removed = contents.Remove(e);
            OnRemove(e);
            return removed;
        }

        public void RemoveAt(int index)
        {
            var at = contents[index];
            contents.RemoveAt(index);
            OnRemove(at);
        }

        public int IndexOf(GddlElement o)
        {
            return contents.IndexOf(o);
        }

        public void Clear()
        {
            foreach(var e in contents) OnRemove(e);
            contents.Clear();
        }

        public override int GetFormattingComplexity()
        {
            return 2 + contents.Sum(e => e.GetFormattingComplexity());
        }
        #endregion

        #region Implementation
        private readonly List<GddlElement> contents = new List<GddlElement>();

        private void OnAdd(GddlElement e)
        {
            if (e.Parent != null) throw new InvalidOperationException("The element is already assigned to a collection.");
            e.Parent = this;
        }

        private void OnRemove(GddlElement e)
        {
            e.Parent = null;
        }

        #endregion

        #region IList Extras
        public IEnumerator<GddlElement> GetEnumerator()
        {
            return contents.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)contents).GetEnumerator();
        }

        public bool Contains(GddlElement item)
        {
            return contents.Contains(item);
        }

        public void CopyTo(GddlElement[] array, int arrayIndex)
        {
            contents.CopyTo(array, arrayIndex);
        }
        #endregion

        #region Element Implementation
        public override GddlList CopyInternal()
        {
            var collection = new GddlList();
            CopyTo(collection);
            return collection;
        }

        protected override void CopyTo(GddlList other)
        {
            base.CopyTo(other);
            foreach (var e in contents)
            {
                other.Add(e.Copy());
            }
        }

        public override void Resolve(GddlElement root)
        {
            foreach (var el in contents)
            {
                el.Resolve(root);
            }
        }

        public override GddlElement Simplify()
        {
            for (int i = 0; i < contents.Count; i++)
            {
                contents[i] = contents[i].Simplify();
            }

            return this;
        }
        
        public IEnumerable<GddlMap> ByType(string type)
        {
            return contents.OfType<GddlMap>().Where(e => e.TypeName == type);
        }
        #endregion

        #region Equality
        public override bool Equals(object other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other == null || GetType() != other.GetType()) return false;
            return EqualsImpl((GddlList)other);
        }

        public override bool Equals(GddlList other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other == null) return false;
            return EqualsImpl(other);
        }

        private bool EqualsImpl(GddlList other)
        {
            if (!base.EqualsImpl(other))
                return false;

            return Utility.ListEquals(contents, other.contents);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), contents);
        }
        #endregion
    }
}