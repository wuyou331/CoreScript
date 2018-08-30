using System;
using System.IO;
using CoreScript;

namespace ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var script = "var age=18;" +
                         "func main(){" +
                         "    var name = getName();" +
                         "    print(\"my name is \",false);" +
                         "    println(name);  " +
                         "    if false then{" +
                         "        name = name;  " +
                         "    } else {" +
                         "        name = (12+2)*3+456+\"haha\";" +
                         "        println(name); " +
                         "        name= \"first code\";  " +
                         "    }" +
                         "    println(getName()); " +
                         "    println(age);" +
                         "}" +
                         "func print(var str,var repart){" +
                         "    Console.Write(str);" +
                         "    if repart==false then {" +
                         "        return ; " +
                         "    }" +
                         "    Console.Write(str);" +
                         "}" +
                         "func println(var str){" +
                         "    Console.WriteLine(str);" +
                         "}" +
                         "    " +
                         "func getName(){" +
                         "    return \" \\\"wuyou \\\" \";" +
                         "}";
            var scritpEngine = new ScriptEngine();
            scritpEngine.Excute(script);
            Console.ReadLine();
        }
    }
}