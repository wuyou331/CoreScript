using System;
using CoreScript;

namespace ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var script = "var age=18;" +
                         "func main(){" +
                         "  var name = \" \\\"wuyou \\\"\";" +
                         "  print(\"my name is \");" +
                         "  println(name);" +
                         "  if age==name or 1==1 then{" +
                         "      name = name;" +
                         "  } else {" +
                         "      name = (12+2)*3+456+\"haha\";" +
                         "      println(name);" +
                         "      name= \"first code\";" +
                         "  }" +
                         "  println(name);" +
                         "  println(age);" +
                         "}" +
                         "func print(var str){" +
                         "  Console.Write(str);" +
                         "}" +
                         "func println(var str){" +
                         "  Console.WriteLine(str);" +
                         "}";
            var scritpEngine = new ScriptEngine();
            scritpEngine.Excute(script);
            Console.ReadLine();
        }
    }
}