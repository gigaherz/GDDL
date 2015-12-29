using System;
using System.IO;
using GDDL.Config;

namespace GDDL.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = Parser.FromFile("..\\..\\..\\Samples\\Test.txt");
            var parsedData = p.Parse();
            var text = parsedData.ToString(new StringGenerationContext(StringGenerationOptions.Nice));
            //File.WriteAllText("Output.txt", text);
            Console.Write(text);
        }
    }
}
