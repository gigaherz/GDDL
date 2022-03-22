using System;
using GDDL.Serialization;

namespace GDDL.Demo
{
    public static class Program
    {
        public static void Main()
        {
            var p = Gddl.FromFile("Test.txt");
            var parsedData = p.Parse(simplify:false);
            var text = Formatter.Format(parsedData, FormatterOptions.Nice);
            //File.WriteAllText("Output.txt", text);

            var v2 = parsedData.Root["named list"][2..3][..^1];
            var v3 = parsedData.Root.Query("'named list'/[2..3]/[..^1]");
            
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.Write(text);
            Console.Out.Flush();
        }
    }
}
