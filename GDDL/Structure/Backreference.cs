using System.Collections.Generic;
using System.Text;

namespace GDDL.Structure
{
    public class Backreference : Element
    {
        protected List<string> NamePart;

        protected bool Rooted;

        private bool resolved;
        private Element resolvedValue;

        public override Element ResolvedValue
        {
            get { return resolvedValue; }
        }

        public override bool IsResolved
        {
            get { return resolved; }
        }

        internal Backreference(bool rooted, string I)
        {
            Rooted = rooted;
            NamePart = new List<string> {I};
        }

        internal virtual void Append(string I)
        {
            NamePart.Add(I);
        }

        public override string ToString()
        {
            var ss = new StringBuilder();
            int count = 0;
            foreach (var it  in NamePart)
            {
                if (count++ > 0)
                    ss.Append(':');
                ss.Append(it);
            }

            if (IsResolved)
            {
                ss.Append('=');
                if (ResolvedValue == null)
                    ss.Append("NULL");
                else
                    ss.Append(ResolvedValue);
            }

            return ss.ToString();
        }

        public override string ToString(StringGenerationContext ctx)
        {
            return ToString();
        }

        internal override void Resolve(Set root)
        {
            if (IsResolved)
                return;

            Element elm = root;

            foreach (var it in NamePart)
            {
                var s = elm as Set;

                if (s == null)
                    continue;

                NamedElement ne;
                if (s.TryGetByName(it, out ne))
                {
                    elm = ne.Value;
                    continue;
                }

                resolvedValue = null;
                resolved = true;
                return;
            }

            resolvedValue = elm;
            resolved = true;

            if (!elm.IsResolved)
                elm.Resolve(root);

            resolvedValue = elm.ResolvedValue;
        }

        public override Element Simplify()
        {
            if (resolved && resolvedValue != null)
                return resolvedValue;

            return this;
        }

    }
}