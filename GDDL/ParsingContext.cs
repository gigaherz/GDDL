
using System;

namespace GDDL
{
    public class ParsingContext : IEquatable<ParsingContext>, IContextProvider
    {
        public readonly string Filename;
        public readonly int Line;
        public readonly int Column;

        public ParsingContext(string f, int l, int c)
        {
            Filename = f;
            Line = l;
            Column = c;
        }

        public override string ToString()
        {
            return $"{Filename}({Line},{Column})";
        }

        public override bool Equals(object other)
        {
            if (other == this) return true;
            if (other == null || GetType() != other.GetType()) return false;
            return EqualsImpl((ParsingContext)other);
        }

        public bool Equals(ParsingContext other)
        {
            if (other == this) return true;
            if (other == null) return false;
            return EqualsImpl(other);
        }

        private bool EqualsImpl(ParsingContext other)
        {
            return Filename == other.Filename && Line == other.Line && Column == other.Column;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Filename, Line, Column);
        }

        ParsingContext IContextProvider.ParsingContext => this;
    }
}
