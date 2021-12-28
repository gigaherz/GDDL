using GDDL.Serialization;
using System;

namespace GDDL.Test
{
    class Program
    {
        static void Main()
        {
            var p = Parser.FromFile("Test.txt");
            var parsedData = p.Parse();
            var text = Formatter.FormatNice(parsedData);
            //File.WriteAllText("Output.txt", text);

            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.Write(text);
            Console.Out.Flush();
        }
    }
}
