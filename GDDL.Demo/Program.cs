﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using GDDL.Serialization;
using GDDL.Structure;

namespace GDDL.Demo
{
    public static class Program
    {
        public static void Main()
        {
            var doc = Gddl.FromFile("Test.txt");
            var text = Formatter.Format(doc, FormatterOptions.Nice);
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.Write(text);
            Console.Out.Flush();
            //File.WriteAllText("Output.txt", text);

            var v2 = doc.Root["named list"][0..^0];
            Console.WriteLine("[" + string.Join(",", v2) + "]");

            var v3 = doc.Root.Query("'named list'/[0..^0]");
            Console.WriteLine("[" + string.Join(",", v3) + "]");
        }
    }
}