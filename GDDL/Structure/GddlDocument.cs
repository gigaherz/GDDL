using GDDL.Serialization;
using System;

namespace GDDL.Structure
{
    public sealed class GddlDocument : IEquatable<GddlDocument>
    {
        #region API

        public static GddlDocument Create()
        {
            return new GddlDocument();
        }

        public static GddlDocument Create(GddlElement root)
        {
            return new GddlDocument(root);
        }

        /// <summary>
        /// The root element of this document.
        /// </summary>
        public GddlElement Root { get; set; }

        /// <summary>
        /// Comment data present after the root element.
        /// </summary>
        public string DanglingComment { get; set; }

        public bool HasDanglingComment => !string.IsNullOrEmpty(DanglingComment);

        #endregion

        #region Implementation

        private GddlDocument()
            : this(GddlMap.Empty())
        {
        }

        private GddlDocument(GddlElement root)
        {
            Root = root;
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return Formatter.FormatCompact(this);
        }

        #endregion

        #region Equality

        public override bool Equals(object other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other == null || GetType() != other.GetType()) return false;
            return EqualsImpl((GddlDocument)other);
        }

        public bool Equals(GddlDocument other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other == null) return false;
            return EqualsImpl(other);
        }

        private bool EqualsImpl(GddlDocument other)
        {
            return Equals(Root, other.Root) && Equals(DanglingComment, other.DanglingComment);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Root, DanglingComment);
        }

        #endregion
    }
}