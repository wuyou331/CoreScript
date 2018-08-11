# CoreScript
C#编写的脚本语言库，目标是作为程序的胶水语言，可以调用.Net的功能，也可以编写方法注册进脚本引擎；<br/>
该项目为实验性项目，用Sprache库作为词法解析库；<br/>
目前还处在起步阶段使,，目前仅实现了一下特性：
* 全局变量定义
* 函数定义
* 函数调用
* main入口函数调用
* .Net框架资源调用


```C#
string script = "var age=18;" +
                "" +
                "func main(){" +
                "var name = \" \\\"wuyou \\\"\";" +
                "Console.Write(\"my name is \");" +
                "Console.WriteLine(name);" +
                "name = 12345;"+
                "Console.WriteLine(name);" +
                "test();"+
                "}" +
                "" +
                "func test(){" +
                "Console.WriteLine(age);" +
                "}";
var scritpEngine = new ScriptEngine();
scritpEngine.Excute(script);
Console.ReadLine();
```

