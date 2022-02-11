using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GDDL.Util;

namespace GDDL.Structure
{
    public sealed class GddlList : Element<GddlList>, IList<GddlElement>
    {
        #region Factory Methods
        public static Collection Empty()
        {
            return new Collection();
        }
        public static Collection Of(params GddlElement[] initial)
        {
            return new Collection(initial);
        }

        public static Collection CopyOf(IEnumerable<GddlElement> initial)
        {
            return new Collection(initial);
        }
        #endregion

        #region Implementation
        private readonly List<GddlElement> contents = new List<GddlElement>();
        
        public int Count => contents.Count;

        public bool IsReadOnly => false;

        public bool IsEmpty => contents.Count == 0;
        public bool IsSimple => !contents.Any(a => a is GddlMap || a is GddlList);
        
        public GddlElement this[int index]
        {
            get => contents[index];
            set
            {
                var old = contents[index];
                contents[index] = value;
            }
        }

        private GddlList()
        {
        }

        private GddlList(IEnumerable<GddlElement> init)
        {
            AddRange(init);
        }
        
        public void Add(GddlElement e)
        {
            contents.Add(e);
        }
        
        public void Insert(int before, GddlElement e)
        {
            contents.Insert(before, e);
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
            return contents.Remove(e);
        }

        public void RemoveAt(int index)
        {
            contents.RemoveAt(index);
        }
        
        public int IndexOf(GddlElement o)
        {
            return contents.IndexOf(o);
        }

        public void Clear()
        {
            contents.Clear();
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
            if (other == this) return true;
            if (other == null || GetType() != other.GetType()) return false;
            return EqualsImpl((GddlList)other);
        }

        public override bool Equals(GddlList other)
        {
            if (other == this) return true;
            if (other == null) return false;
            return EqualsImpl(other);
        }

        private bool EqualsImpl(GddlList other)
        {
            return base.EqualsImpl(other) &&
                   Equals(contents, other.contents);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), contents);
        }
        #endregion
    }
}