using System.Collections.Generic;
using System.Text;

namespace GDDL.Config
{
    public class StringGenerationContext
    {
        public StringGenerationOptions Options;

        private readonly Stack<int> IndentLevels = new Stack<int>();

        public int IndentLevel = 1;

        public StringGenerationContext(StringGenerationOptions options)
        {
            Options = options;
        }


        public void PushIndent()
        {
            IndentLevels.Push(IndentLevel);
        }

        public void PopIndent()
        {
            IndentLevel = IndentLevels.Pop();
        }

        public void SetIndent(int newIndent)
        {
            IndentLevel = newIndent;
        }

        public void IncIndent()
        {
            IndentLevel++;
        }

        public void AppendIndent(StringBuilder builder)
        {
            int tabsToGen = IndentLevel;
            for (int i = 0; i < tabsToGen; i++)
            {
                if (Options.indentUsingTabs)
                {
                    builder.Append("\t");
                }
                else
                {
                    for (int j = 0; j < Options.spacesPerIndent; j++)
                    {
                        builder.Append(" ");
                    }
                }
            }
        }
    }
}