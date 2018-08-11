using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreScript.Script;
using CoreScript.Tokens;
using Sprache;

namespace CoreScript
{
    public class ScriptEngine
    {
        public  IDictionary<string,ScriptVariable> GlobalVariable { get;  }= new ConcurrentDictionary<string, ScriptVariable>();
        public ScriptEngine()
        {
        }

        public void Excute(string script)
        {
            var rs = Lexer.Analyzer(script);
            foreach (var token in rs.Where(it => it.TokenType == TokenType.Assignment))
            {
                
            }

            var main = rs.Where(it => it.TokenType == TokenType.FunctionDefine)
                .Cast<TokenFunctionDefine>()
                .Single(it => it.Name == "main");



            ScriptFunction func = new ScriptFunction(main, this);
            func.Excute(null);
        }
    }
}