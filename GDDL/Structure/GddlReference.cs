using System;
using System.Collections.Generic;
using System.Linq;
using GDDL.Util;

namespace GDDL.Structure
{
    public sealed class GddlReference : GddlElement<GddlReference>
    {
        #region API

        public static GddlReference Of(QueryPath queryPath)
        {
            return new GddlReference(queryPath);
        }

        public override bool IsReference => true;
        public override GddlReference AsReference => this;
        
        public override bool IsResolved => resolved;
        public override GddlElement ResolvedValue => resolvedValue;

        public GddlReference(QueryPath path)
        {
            this.path = path;
        }

        public bool IsAbsolute => path.IsAbsolute;

        public IReadOnlyList<QueryComponent> NameParts => path.PathComponents;
        #endregion

        #region Implementation
        private readonly QueryPath path;

        private bool resolved;
        private GddlElement resolvedValue;
        #endregion

        #region Element

        public override GddlReference CopyInternal()
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

            resolved = TryResolve(root, !IsAbsolute);
        }

        private bool TryResolve(GddlElement root, bool relative)
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

            var result = new Query(target);

            foreach (var part in NameParts)
            {
                result = part.Filter(result);
            }

            target = result.Targets.Single();

            if (!target.IsResolved)
                target.Resolve(root);

            resolvedValue = target.ResolvedValue;

            if (ReferenceEquals(resolvedValue, this))
                throw new InvalidOperationException("Invalid cyclic reference: Reference resolves to itself.");

            while (parent != null)
            {
                if (ReferenceEquals(resolvedValue, parent))
                    throw new InvalidOperationException("Invalid cyclic reference: Reference resolves to a parent of the current element.");
                parent = parent.Parent;
            }

            return resolvedValue != null;
        }

        public override GddlElement Simplify()
        {
            if (resolved && resolvedValue != null)
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
                Equals(path, other.path) &&
                (IsResolved
                    ? other.IsResolved && Equals(ResolvedValue, other.ResolvedValue)
                    : !other.IsResolved);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), IsResolved, ResolvedValue, path);
        }
        #endregion
    }
}