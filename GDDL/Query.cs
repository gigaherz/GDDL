using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GDDL.Parsing;
using GDDL.Serialization;
using GDDL.Structure;
using GDDL.Util;

namespace GDDL
{
    public class Query
    {
        public IEnumerable<GddlElement> Targets { get; }

        public Query(params GddlElement[] targets)
        {
            Targets = targets;
        }

        public Query(IEnumerable<GddlElement> targets)
        {
            Targets = targets;
        }
    }

    public static class QueryParser
    {
        public static QueryPath ParsePath(string pathExpression)
        {
            var reader = new Reader(new StringReader(pathExpression), "QueryParser.ParsePath(string)");
            var lexer = new Lexer(reader);
            var parser = new Parser(lexer);
            return parser.ParseQuery();
        }
    }

    public class QueryPath
    {
        private bool absolute = false;
        private readonly List<QueryComponent> pathComponents = new List<QueryComponent>();

        public bool IsAbsolute => absolute;
        public IReadOnlyList<QueryComponent> PathComponents => pathComponents.AsReadOnly();

        public QueryPath Absolute()
        {
            absolute = true;
            return this;
        }

        public QueryPath ByKey(string name)
        {
            pathComponents.Add(new MapQueryComponent(name));
            return this;
        }

        public QueryPath ByRange(Range range)
        {
            pathComponents.Add(new ListQueryComponent(range));
            return this;
        }

        public QueryPath Self()
        {
            pathComponents.Add(SelfQueryComponent.Instance);
            return this;
        }

        public QueryPath Parent()
        {
            pathComponents.Add(ParentQueryComponent.Instance);
            return this;
        }

        public QueryPath Copy()
        {
            var path = new QueryPath();
            CopyTo(path);
            return path;
        }

        public void CopyTo(QueryPath otherPath)
        {
            foreach(var component in pathComponents)
            {
                otherPath.pathComponents.Add(component.Copy());
            }
        }
    }

    public abstract class QueryComponent
    {
        public abstract Query Filter(Query input);
        
        public abstract string ToString(Formatter formatter);

        public abstract QueryComponent Copy();
    }

    public class MapQueryComponent : QueryComponent
    {
        public string Name { get; }

        public MapQueryComponent(string name)
        {
            Name = name;
        }

        public override Query Filter(Query input)
        {
            return new Query(input.Targets
                .OfType<GddlMap>()
                .Where(m => m.ContainsKey(Name))
                .Select(m => m[Name]));
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
    }

    public class ListQueryComponent : QueryComponent
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

        public override Query Filter(Query input)
        {
            return new Query(input.Targets
                .OfType<GddlList>()
                .SelectMany(m =>
                {
                    var start = range.Start.Value;
                    var end = range.End.Value;
                    if (range.Start.IsFromEnd) start = m.Count - start;
                    if (range.End.IsFromEnd) end = m.Count - end;
                    return m.Skip(start).Take(end-start);
                }));
        }

        public override string ToString(Formatter formatter)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            var start = range.Start;
            var end = range.End;
            if (start.Value != 0 || start.IsFromEnd)
            {
                if (start.IsFromEnd) sb.Append("^");
                sb.Append(start.Value);
                if (end.IsFromEnd == start.IsFromEnd)
                {
                    if ((!start.IsFromEnd && (start.Value + 1 == end.Value)) || (start.IsFromEnd && (start.Value == end.Value + 1)))
                    {
                        sb.Append("]");
                        return sb.ToString();
                    }
                }
            }
            sb.Append("..");
            if (end.Value != 0 || end.IsFromEnd)
            {
                if (end.IsFromEnd) sb.Append("^");
                sb.Append(end.Value);
            }
            sb.Append("]");
            return sb.ToString();
        }

        public override QueryComponent Copy()
        {
            return new ListQueryComponent(range);
        }
    }
    
    public class SelfQueryComponent : QueryComponent
    {
        public static SelfQueryComponent Instance { get; } = new SelfQueryComponent();

        private SelfQueryComponent()
        {
        }

        public override Query Filter(Query input)
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
    }

    public class ParentQueryComponent : QueryComponent
    {
        public static ParentQueryComponent Instance { get; } = new ParentQueryComponent();

        private ParentQueryComponent()
        {
        }

        public override Query Filter(Query input)
        {
            return new Query(input.Targets
                .Where(t => t.Parent != null)
                .Select(t => t.Parent));
        }

        public override string ToString(Formatter formatter)
        {
            return "..";
        }

        public override QueryComponent Copy()
        {
            return this;
        }
    }
}
