﻿using System;
using CoreScript;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string script = "func main(){" +
                            "String name = \" \\\"wuyou \\\"\";" +
                            "Console.Write(\"my name is \");" +
                            "Console.WriteLine(name);" +
                            "Console.WriteLine(123);" +
                            "Console.WriteLine(123.12);" +
                            "}";
            var scritpEngine = new ScriptEngine();
            scritpEngine.Excute(script);
            Console.ReadLine();
        }
    }
}
