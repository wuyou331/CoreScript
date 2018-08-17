using System;
using System.IO;
using CoreScript;

namespace ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var script = File.ReadAllText("script.cj");
            var scritpEngine = new ScriptEngine();
            scritpEngine.Excute(script);
            Console.ReadLine();
        }
    }
}