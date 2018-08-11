using System;
using CoreScript;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string script = "var age=18;" +
                            "" +
                            "func main(){" +
                            "var name = \" \\\"wuyou \\\"\";" +
                            "Console.Write(\"my name is \");" +
                            "Console.WriteLine(name);" +
                            "name = 12345;"+
                            "Console.WriteLine(name);" +
                            "}" +
                            "" +
                            "func test(){" +
                            "Console.WriteLine(age);" +
                            "}";
            var scritpEngine = new ScriptEngine();
            scritpEngine.Excute(script);
            Console.ReadLine();
        }
    }
}
