using GDDL.Parsing;
using GDDL.Serialization;
using GDDL.Structure;
using GDDL.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GDDL.Queries
{
    public class Query : IEquatable<Query>
    {
        public static Query FromString(string pathExpression)
        {
            var reader = new Reader(new StringReader(pathExpression), "QueryParser.ParsePath(string)");
            var lexer = new Lexer(reader);
            var parser = new Parser(lexer);
            return parser.ParseQuery();
        }

        private bool absolute = false;
        private readonly List<QueryComponent> pathComponents = [];

        public bool IsAbsolute => absolute;
        public IReadOnlyList<QueryComponent> PathComponents => pathComponents.AsReadOnly();

        public Query Absolute()
        {
            if (PathComponents.Count > 0)
                throw new InvalidOperationException("Cannot set Absolute after path components have been added.");
            absolute = true;
            return this;
        }

        public Query ByKey(string name)
        {
            pathComponents.Add(new MapQueryComponent(name));
            return this;
        }

        public Query ByRange(Range range)
        {
            pathComponents.Add(new ListQueryComponent(range));
            return this;
        }

        public Query Self()
        {
            pathComponents.Add(SelfQueryComponent.Instance);
            return this;
        }

        public Query Parent()
        {
            pathComponents.Add(ParentQueryComponent.Instance);
            return this;
        }

        public IEnumerable<GddlElement> Apply(GddlElement target)
        {
            var result = Enumerable.Repeat(target, 1);

            foreach (var part in pathComponents)
            {
                result = part.Filter(result);
            }

            return result;
        }

        public Query Copy()
        {
            var path = new Query();
            CopyTo(path);
            return path;
        }

        public void CopyTo(Query other)
        {
            foreach (var component in pathComponents)
            {
                other.pathComponents.Add(component.Copy());
            }
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null || GetType() != other.GetType()) return false;
            return EqualsImpl((Query)other);
        }

        public bool Equals(Query other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            return EqualsImpl(other);
        }

        private bool EqualsImpl(Query other)
        {
            return absolute == other.absolute && Utility.ListEquals(pathComponents, other.pathComponents);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(absolute, pathComponents);
        }
    }

    public abstract class QueryComponent
    {
        public abstract IEnumerable<GddlElement> Filter(IEnumerable<GddlElement> input);

        public abstract string ToString(Formatter formatter);

        public abstract QueryComponent Copy();

        public abstract override bool Equals(object other);

        public abstract override int GetHashCode();
    }

    public class MapQueryComponent(string name) : QueryComponent, IEquatable<MapQueryComponent>
    {
        public string Name => name;

        public override IEnumerable<GddlElement> Filter(IEnumerable<GddlElement> input)
        {
            return input
                .OfType<GddlMap>()
                .Where(m => m.ContainsKey(Name))
                .Select(m => m[Name]);
        }

        public override string ToString(Formatter formatter)
        {
            var name = Name;
            if (formatter.Options.alwaysUseStringLiterals || !Utility.IsValidIdentifier(name))
                name = Utility.EscapeString(name);
            return name;
        }

        public override QueryComponent Copy()
        {
            return new MapQueryComponent(Name);
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other == null || GetType() != other.GetType()) return false;
            return EqualsImpl((MapQueryComponent)other);
        }

        public bool Equals(MapQueryComponent other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other == null) return false;
            return EqualsImpl(other);
        }

        private bool EqualsImpl(MapQueryComponent other)
        {
            return Name == other.Name;
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }

    public class ListQueryComponent : QueryComponent, IEquatable<ListQueryComponent>
    {
        private readonly Range range;

        public ListQueryComponent(int start, bool fromEnd, int second)
        {
            range = new Range(Index.FromStart(start), new Index(second, fromEnd));
        }

        public ListQueryComponent(Range range)
        {
            this.range = range;
        }

        public override IEnumerable<GddlElement> Filter(IEnumerable<GddlElement> input)
        {
            return input
                .OfType<GddlList>()
                .SelectMany(m =>
                {
                    var start = range.Start.Value;
                    var end = range.End.Value;
                    if (range.Start.IsFromEnd) start = m.Count - start;
                    if (range.End.IsFromEnd) end = m.Count - end;
                    return m.Skip(start).Take(end - start);
                });
        }

        public override string ToString(Formatter formatter)
        {
            var sb = new StringBuilder();
            sb.Append('[');
            var start = range.Start;
            var end = range.End;
            if (start.Value != 0 || start.IsFromEnd)
            {
                if (start.IsFromEnd) sb.Append('^');
                sb.Append(start.Value);
                if (end.IsFromEnd == start.IsFromEnd)
                {
                    if ((!start.IsFromEnd && (start.Value + 1 == end.Value)) ||
                        (start.IsFromEnd && (start.Value == end.Value + 1)))
                    {
                        sb.Append(']');
                        return sb.ToString();
                    }
                }
            }

            sb.Append("..");
            if (end.Value != 0 || end.IsFromEnd)
            {
                if (end.IsFromEnd) sb.Append('^');
                sb.Append(end.Value);
            }

            sb.Append(']');
            return sb.ToString();
        }

        public override QueryComponent Copy()
        {
            return new ListQueryComponent(range);
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other == null || GetType() != other.GetType()) return false;
            return EqualsImpl((ListQueryComponent)other);
        }

        public bool Equals(ListQueryComponent other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other == null) return false;
            return EqualsImpl(other);
        }

        private bool EqualsImpl(ListQueryComponent other)
        {
            return range.Equals(other.range);
        }

        public override int GetHashCode()
        {
            return range.GetHashCode();
        }
    }

    public class SelfQueryComponent : QueryComponent
    {
        public static SelfQueryComponent Instance { get; } = new();

        private SelfQueryComponent()
        {
        }

        public override IEnumerable<GddlElement> Filter(IEnumerable<GddlElement> input)
        {
            return input;
        }

        public override string ToString(Formatter formatter)
        {
            return ".";
        }

        public override QueryComponent Copy()
        {
            return this;
        }

        public override bool Equals(object other)
        {
            return other == this;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }

    public class ParentQueryComponent : QueryComponent
    {
        public static ParentQueryComponent Instance { get; } = new();

        private ParentQueryComponent()
        {
        }

        public override IEnumerable<GddlElement> Filter(IEnumerable<GddlElement> input)
        {
            return input
                .Where(t => t.Parent != null)
                .Select(t => t.Parent);
        }

        public override string ToString(Formatter formatter)
        {
            return "..";
        }

        public override QueryComponent Copy()
        {
            return this;
        }

        public override bool Equals(object other)
        {
            return other == this;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}