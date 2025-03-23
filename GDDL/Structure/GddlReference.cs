using System;
using System.Collections.Generic;
using System.Linq;
using GDDL.Exceptions;
using GDDL.Queries;
using Query = GDDL.Queries.Query;

namespace GDDL.Structure
{
    public sealed class GddlReference(Query path) : GddlElement<GddlReference>
    {
        #region API

        public static GddlReference Of(Query query)
        {
            return new GddlReference(query);
        }

        public override bool IsReference => true;
        public override GddlReference AsReference => this;

        public override bool IsResolved => resolvedValue != null;
        public override GddlElement ResolvedValue => resolvedValue;

        public bool IsAbsolute => path.IsAbsolute;

        public IReadOnlyList<QueryComponent> NameParts => path.PathComponents;

        #endregion

        #region Implementation

        private readonly Query path = path;

        private GddlElement resolvedValue;

        #endregion

        #region Element

        protected override GddlReference CopyInternal()
        {
            var reference = new GddlReference(path.Copy());
            CopyTo(reference);
            return reference;
        }

        protected override void CopyTo(GddlReference other)
        {
            base.CopyTo(other);
            path.CopyTo(other.path);
        }

        public override void Resolve(GddlElement root)
        {
            if (IsResolved)
                return;

            TryResolve(root, !IsAbsolute);
        }

        private void TryResolve(GddlElement root, bool relative)
        {
            try
            {
                var parent = Parent;

                GddlElement target;
                if (relative)
                {
                    target = parent ?? this;
                }
                else
                {
                    target = root;
                }

                target = path.Apply(target).SingleOrDefault();

                if (target != null)
                {
                    if (!target.IsResolved)
                        target.Resolve(root);

                    resolvedValue = target.ResolvedValue;

                    if (resolvedValue != null)
                    {
                        if (ReferenceEquals(resolvedValue, this))
                            throw new InvalidOperationException(
                                "Invalid cyclic reference: Reference resolves to itself.");

                        while (parent != null)
                        {
                            if (ReferenceEquals(resolvedValue, parent))
                                throw new InvalidOperationException(
                                    "Invalid cyclic reference: Reference resolves to a parent of the current element.");
                            parent = parent.Parent;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ResolutionException("Error resolving reference '" + this + "'", ex);
            }
        }

        public override GddlElement Simplify()
        {
            if (resolvedValue != null)
                return resolvedValue.Copy();

            return this;
        }

        #endregion

        #region Equality

        public override bool Equals(object other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other == null || GetType() != other.GetType()) return false;
            return EqualsImpl((GddlReference)other);
        }

        public override bool Equals(GddlReference other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other == null) return false;
            return EqualsImpl(other);
        }

        private bool EqualsImpl(GddlReference other)
        {
            return base.EqualsImpl(other) &&
                   Equals(path, other.path) /*&&
                (IsResolved
                    ? other.IsResolved && Equals(ResolvedValue, other.ResolvedValue)
                    : !other.IsResolved)*/;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), path);
        }

        #endregion
    }
}